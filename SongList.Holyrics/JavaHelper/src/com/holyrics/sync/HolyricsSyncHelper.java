package com.holyrics.sync;

import com.limagiran.googledrive.SyncBytesItem;
import com.limagiran.googledrive.SyncBytesList;
import com.limagiran.holyrics.model.DeletedItem;
import com.limagiran.holyrics.model.Music;
import java.io.ByteArrayOutputStream;
import java.io.ByteArrayInputStream;
import java.io.DataInputStream;
import java.io.ObjectInputStream;
import java.io.ObjectOutputStream;
import java.nio.charset.StandardCharsets;
import java.security.MessageDigest;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import javax.crypto.Cipher;
import javax.crypto.spec.IvParameterSpec;
import javax.crypto.spec.SecretKeySpec;

public class HolyricsSyncHelper {
   private static final String KEY_OBJ = "$%#@keyOBJ*&\u00A8123";
   private static final String IV = "AAAAAAAAAAAAAAAA";

   public static void main(String[] args) throws Exception {
      Args parsed = Args.parse(args);
      if (parsed.applyUpdates) {
         ApplyRequest req = readApplyRequestFromStdin();
         ApplyResult result = applyUpdates(req);
         System.out.println(HolyricsJsonWriter.applyResultToJson(result.name, result.customMd5, result.bytes));
         return;
      }
      if (parsed.readDeleted) {
         if (!parsed.useStdin) {
            System.err.println("Usage: --deleted --stdin");
            System.exit(2);
            return;
         }
         byte[] bytes = readSingleBlobFromStdin();
         Map<String, DeletedItem> deleted = readDeletedItems(bytes);
         System.out.println(HolyricsJsonWriter.deletedToJson(deleted));
         return;
      }
      if (parsed.useStdin) {
         byte[] fileBytes = readSingleBlobFromStdin();
         SyncBytesList list = readSyncBytesList(fileBytes);
         Map<String, SongData> songs = new LinkedHashMap();
         for (Object obj : list.getItems()) {
            if (!(obj instanceof SyncBytesItem)) {
               continue;
            }
            SyncBytesItem item = (SyncBytesItem)obj;
            Music music = readMusic(item.getBytes());
            if (music == null) {
               continue;
            }
            SongData data = new SongData(item, music);
            songs.put(item.getId(), data);
         }
         System.out.println(HolyricsJsonWriter.songsToJson(songs.values()));
         return;
      }

      System.err.println("Usage: --stdin | --deleted --stdin | --apply");
      System.exit(2);
   }

   private static SyncBytesList readSyncBytesList(byte[] bytes) throws Exception {
      Object obj = readObject(bytes);
      if (!(obj instanceof SyncBytesList)) {
         throw new IllegalStateException("Invalid SyncBytesList in bytes");
      }
      return (SyncBytesList)obj;
   }

   private static Map<String, DeletedItem> readDeletedItems(byte[] bytes) throws Exception {
      Object obj = readObject(bytes);
      Map<String, DeletedItem> result = new HashMap();
      if (obj instanceof Map) {
         Map map = (Map)obj;
         for (Object entryObj : map.entrySet()) {
            Map.Entry entry = (Map.Entry)entryObj;
            if (entry.getKey() instanceof String && entry.getValue() instanceof DeletedItem) {
               result.put((String)entry.getKey(), (DeletedItem)entry.getValue());
            }
         }
      }
      return result;
   }

   private static Music readMusic(byte[] encrypted) throws Exception {
      if (encrypted == null || encrypted.length == 0) {
         return null;
      }
      byte[] decrypted = decryptBytes(encrypted, KEY_OBJ);
      Object obj = readObject(decrypted);
      return obj instanceof Music ? (Music)obj : null;
   }

   private static Object readObject(byte[] bytes) throws Exception {
      ByteArrayInputStream bais = new ByteArrayInputStream(bytes);
      ObjectInputStream ois = new ObjectInputStream(bais);
      try {
         return ois.readObject();
      } finally {
         ois.close();
      }
   }

   private static byte[] decryptBytes(byte[] cipherText, String key) throws Exception {
      String pass16 = getPass16(key);
      Cipher cip = Cipher.getInstance("AES/CBC/PKCS5Padding");
      SecretKeySpec keySpec = new SecretKeySpec(pass16.getBytes(StandardCharsets.UTF_8), "AES");
      cip.init(Cipher.DECRYPT_MODE, keySpec, new IvParameterSpec(IV.getBytes(StandardCharsets.UTF_8)));
      return cip.doFinal(cipherText);
   }

   private static String getPass16(String key) throws Exception {
      if (key == null || key.isEmpty()) {
         key = "A";
      }
      MessageDigest sha256 = MessageDigest.getInstance("SHA-256");
      byte[] digest = sha256.digest(key.getBytes(StandardCharsets.UTF_8));
      return toHexUpper(digest).substring(0, 16);
   }

   private static String toHexUpper(byte[] bytes) {
      StringBuilder sb = new StringBuilder(bytes.length * 2);
      for (byte b : bytes) {
         int v = b & 0xFF;
         char hi = Character.toUpperCase(Character.forDigit(v >>> 4, 16));
         char lo = Character.toUpperCase(Character.forDigit(v & 0x0F, 16));
         sb.append(hi).append(lo);
      }
      return sb.toString();
   }

   private static byte[] readSingleBlobFromStdin() throws Exception {
      DataInputStream in = new DataInputStream(System.in);
      int dataLen = in.readInt();
      if (dataLen < 0) {
         throw new IllegalStateException("Invalid data length");
      }
      byte[] data = new byte[dataLen];
      in.readFully(data);
      return data;
   }

   private static byte[] writeObjectBytes(Object obj) throws Exception {
      ByteArrayOutputStream baos = new ByteArrayOutputStream();
      ObjectOutputStream oos = new ObjectOutputStream(baos);
      try {
         oos.writeObject(obj);
      } finally {
         oos.close();
      }
      return baos.toByteArray();
   }

   private static boolean isMusicDbName(String name) {
      if (name == null || name.isEmpty()) {
         return false;
      }
      if (name.startsWith("K8ABZWM4ZjYnxb1f5OXYZk9x0RbiRjlKwB33QrY9_")) {
         name = name.substring("K8ABZWM4ZjYnxb1f5OXYZk9x0RbiRjlKwB33QrY9_".length());
      }
      return name.startsWith("music_") && name.endsWith(".db");
   }

   private static ApplyRequest readApplyRequestFromStdin() throws Exception {
      DataInputStream in = new DataInputStream(System.in);
      int fileCount = in.readInt();
      List<byte[]> files = new ArrayList();
      for (int i = 0; i < fileCount; i++) {
         int dataLen = in.readInt();
         if (dataLen < 0) {
            throw new IllegalStateException("Invalid data length");
         }
         byte[] data = new byte[dataLen];
         in.readFully(data);
         files.add(data);
      }
      int updateCount = in.readInt();
      List<UpdateItem> updates = new ArrayList();
      for (int i = 0; i < updateCount; i++) {
         String syncId = readStringU16(in);
         long id = in.readLong();
         long modifiedTime = in.readLong();
         String title = readStringI32(in);
         String artist = readStringI32(in);
         String lyrics = readStringI32(in);
         String author = readStringI32(in);
         String note = readStringI32(in);
         updates.add(new UpdateItem(syncId, id, modifiedTime, title, artist, lyrics, author, note));
      }
      return new ApplyRequest(files, updates);
   }

   private static String readStringU16(DataInputStream in) throws Exception {
      int len = in.readUnsignedShort();
      if (len == 0) {
         return "";
      }
      byte[] data = new byte[len];
      in.readFully(data);
      return new String(data, StandardCharsets.UTF_8);
   }

   private static String readStringI32(DataInputStream in) throws Exception {
      int len = in.readInt();
      if (len < 0) {
         return null;
      }
      if (len == 0) {
         return "";
      }
      byte[] data = new byte[len];
      in.readFully(data);
      return new String(data, StandardCharsets.UTF_8);
   }

   private static ApplyResult applyUpdates(ApplyRequest request) throws Exception {
      if (request == null || request.files.isEmpty()) {
         throw new IllegalStateException("No files provided for apply.");
      }

      byte[] musicBytes = request.files.get(0);
      if (musicBytes == null) {
         throw new IllegalStateException("No music bytes provided.");
      }

      SyncBytesList list = readSyncBytesList(musicBytes);
      Map items = list.getItemsMap();

      for (UpdateItem update : request.updates) {
         if (update == null) {
            continue;
         }
         long id = update.id;
         if (id <= 0L) {
            id = System.currentTimeMillis();
         }
         String syncId = (update.syncId == null || update.syncId.isEmpty())
               ? Long.toString(id, 16)
               : update.syncId;

         SyncBytesItem existing = (SyncBytesItem)items.get(syncId);
         Music music = existing != null ? readMusic(existing.getBytes()) : null;
         if (music == null) {
            music = new Music();
            music.setId(id);
            music.setSet(System.currentTimeMillis());
         }

         if (update.title != null) {
            music.setTitle(update.title);
         }
         if (update.artist != null) {
            music.setArtist(update.artist);
         }
         if (update.lyrics != null) {
            music.setLyrics(update.lyrics);
         }
         if (update.author != null) {
            music.setAuthor(update.author);
         }
         if (update.note != null) {
            music.setNote(update.note);
         }

         long modifiedTime = update.modifiedTime > 0L ? update.modifiedTime : System.currentTimeMillis();
         music.setModifiedTime(modifiedTime);
         music.setVersion(2L);

         byte[] musicBytesEnc = encryptBytes(writeObjectBytes(music), KEY_OBJ);
         String title = music.getTitle();
         SyncBytesItem newItem = new SyncBytesItem(syncId, title, musicBytesEnc, modifiedTime);
         items.put(syncId, newItem);
      }

      list.invalidateCache();
      String customMd5 = list.getMD5();
      byte[] updatedBytes = writeObjectBytes(list);
      return new ApplyResult("", customMd5, updatedBytes);
   }

   private static byte[] encryptBytes(byte[] plainText, String key) throws Exception {
      String pass16 = getPass16(key);
      Cipher cip = Cipher.getInstance("AES/CBC/PKCS5Padding");
      SecretKeySpec keySpec = new SecretKeySpec(pass16.getBytes(StandardCharsets.UTF_8), "AES");
      cip.init(Cipher.ENCRYPT_MODE, keySpec, new IvParameterSpec(IV.getBytes(StandardCharsets.UTF_8)));
      return cip.doFinal(plainText);
   }


   static final class SongData {
      final String syncId;
      final long id;
      final String title;
      final String artist;
      final String lyrics;
      final List order;
      final String author;
      final String note;
      final String language;
      final String formatting;
      final long set;
      final String titleToSearch;
      final String artistToSearch;
      final String lyricsToSearch;
      final String noteToSearch;
      final String lyricsHTML;
      final String titleHTML;
      final String artistHTML;
      final String src;
      final Integer numIni;
      final String[][] translations;
      final boolean archived;
      final long modifiedTime;
      final long itemModifiedTime;
      final String midi;
      final String[] strParams;
      final long version;

      private SongData(SyncBytesItem item, Music music) {
         this.syncId = item.getId();
         this.id = music.getId();
         this.title = music.getTitle();
         this.artist = music.getArtist();
         this.lyrics = music.getLyrics();
         this.order = music.getOrder();
         this.author = music.getAuthor();
         this.note = music.getNote();
         this.language = music.getLanguage();
         this.formatting = music.getFormatting();
         this.set = music.getSet();
         this.titleToSearch = music.getTitleToSearch();
         this.artistToSearch = music.getArtistToSearch();
         this.lyricsToSearch = music.getLyricsToSearch();
         this.noteToSearch = music.getNoteToSearch();
         this.lyricsHTML = music.getLyricsHTML();
         this.titleHTML = music.getTitleHTML();
         this.artistHTML = music.getArtistHTML();
         this.src = music.getSrc();
         this.numIni = music.getNumIni();
         this.translations = music.getTranslations();
         this.archived = music.isArchived();
         this.modifiedTime = music.getModifiedTime();
         this.itemModifiedTime = item.getModifiedTime();
         this.midi = music.getMidi();
         this.strParams = music.getStrParams();
         this.version = music.getVersion();
      }

   }

   private static final class Args {
      private boolean useStdin;
      private boolean applyUpdates;
      private boolean readDeleted;

      private static Args parse(String[] args) {
         Args parsed = new Args();
         for (int i = 0; i < args.length; i++) {
            String arg = args[i];
            if ("--apply".equals(arg)) {
               parsed.applyUpdates = true;
               continue;
            }
            if ("--stdin".equals(arg)) {
               parsed.useStdin = true;
               continue;
            }
            if ("--deleted".equals(arg)) {
               parsed.readDeleted = true;
            }
         }
         return parsed;
      }
   }

   private static final class UpdateItem {
      private final String syncId;
      private final long id;
      private final long modifiedTime;
      private final String title;
      private final String artist;
      private final String lyrics;
      private final String author;
      private final String note;

      private UpdateItem(String syncId, long id, long modifiedTime, String title, String artist, String lyrics, String author, String note) {
         this.syncId = syncId;
         this.id = id;
         this.modifiedTime = modifiedTime;
         this.title = title;
         this.artist = artist;
         this.lyrics = lyrics;
         this.author = author;
         this.note = note;
      }
   }

   private static final class ApplyRequest {
      private final List<byte[]> files;
      private final List<UpdateItem> updates;

      private ApplyRequest(List<byte[]> files, List<UpdateItem> updates) {
         this.files = files;
         this.updates = updates;
      }
   }

   private static final class ApplyResult {
      private final String name;
      private final String customMd5;
      private final byte[] bytes;

      private ApplyResult(String name, String customMd5, byte[] bytes) {
         this.name = name;
         this.customMd5 = customMd5;
         this.bytes = bytes;
      }
   }
}
