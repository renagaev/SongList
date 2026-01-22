package com.holyrics.sync;

import com.limagiran.googledrive.AuthToken;
import java.io.FileInputStream;
import java.io.ObjectInputStream;
import java.nio.charset.StandardCharsets;
import java.io.InputStream;
import java.nio.file.Files;
import java.nio.file.Path;
import java.security.MessageDigest;
import javax.crypto.Cipher;
import javax.crypto.CipherInputStream;
import javax.crypto.spec.IvParameterSpec;
import javax.crypto.spec.SecretKeySpec;

public class HolyricsTokenHelper {
   private static final String TOKEN_KEY = "&p4ssS3sp";
   private static final String IV = "AAAAAAAAAAAAAAAA";

   public static void main(String[] args) throws Exception {
      Args parsed = Args.parse(args);
      Path tokenFile = parsed.tokenFile != null ? parsed.tokenFile : findTokenFile(parsed.holyricsHome);
      if (tokenFile == null || !Files.exists(tokenFile)) {
         System.err.println("Token file not found. Use --file <path> or --holyrics-home <path>.");
         System.exit(2);
      }

      AuthToken token = readToken(tokenFile);
      if (token == null) {
         System.err.println("Failed to read auth token.");
         System.exit(3);
      }

      System.out.println(toJson(token, tokenFile));
   }

   private static AuthToken readToken(Path file) throws Exception {
      try (FileInputStream fis = new FileInputStream(file.toFile());
           CipherInputStream cis = decrypt(fis, TOKEN_KEY);
           ObjectInputStream ois = new ObjectInputStream(cis)) {
         Object obj = ois.readObject();
         return obj instanceof AuthToken ? (AuthToken)obj : null;
      }
   }

   private static CipherInputStream decrypt(InputStream is, String encKey) throws Exception {
      String pass16 = getPass16(encKey);
      Cipher cip = Cipher.getInstance("AES/CBC/PKCS5Padding");
      SecretKeySpec key = new SecretKeySpec(pass16.getBytes(StandardCharsets.UTF_8), "AES");
      cip.init(Cipher.DECRYPT_MODE, key, new IvParameterSpec(IV.getBytes(StandardCharsets.UTF_8)));
      return new CipherInputStream(is, cip);
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

   private static String toJson(AuthToken token, Path path) {
      StringBuilder sb = new StringBuilder(256);
      sb.append("{");
      sb.append("\"path\":\"").append(jsonEscape(path.toString())).append("\",");
      sb.append("\"access_token\":\"").append(jsonEscape(token.getAccess_token())).append("\",");
      sb.append("\"token_type\":\"").append(jsonEscape(token.getToken_type())).append("\",");
      sb.append("\"expires_in\":").append(token.getExpires_in()).append(",");
      sb.append("\"refresh_token\":\"").append(jsonEscape(token.getRefresh_token())).append("\",");
      sb.append("\"scope\":\"").append(jsonEscape(token.getScope())).append("\",");
      sb.append("\"created_at\":").append(token.getCreated_at());
      sb.append("}");
      return sb.toString();
   }

   private static String jsonEscape(String value) {
      if (value == null) {
         return "";
      }
      StringBuilder sb = new StringBuilder(value.length() + 16);
      for (int i = 0; i < value.length(); i++) {
         char c = value.charAt(i);
         switch (c) {
            case '\\':
               sb.append("\\\\");
               break;
            case '"':
               sb.append("\\\"");
               break;
            case '\n':
               sb.append("\\n");
               break;
            case '\r':
               sb.append("\\r");
               break;
            case '\t':
               sb.append("\\t");
               break;
            default:
               if (c < 32) {
                  sb.append(String.format("\\u%04x", (int)c));
               } else {
                  sb.append(c);
               }
         }
      }
      return sb.toString();
   }

   private static Path findTokenFile(Path holyricsHome) throws Exception {
      Path home = holyricsHome != null ? holyricsHome : Path.of(System.getProperty("user.home"), ".holyrics");
      if (!Files.isDirectory(home)) {
         return null;
      }
      Path latest = null;
      long latestTime = -1L;
      try (var stream = Files.list(home)) {
         for (Path p : (Iterable<Path>)stream::iterator) {
            String name = p.getFileName().toString();
            if (name.startsWith("google_drive_auth_token-") && name.endsWith(".dat")) {
               long mtime = Files.getLastModifiedTime(p).toMillis();
               if (mtime > latestTime) {
                  latestTime = mtime;
                  latest = p;
               }
            }
         }
      }
      return latest;
   }

   private static final class Args {
      private Path tokenFile;
      private Path holyricsHome;

      private static Args parse(String[] args) {
         Args parsed = new Args();
         for (int i = 0; i < args.length; i++) {
            String arg = args[i];
            if ("--file".equals(arg) && i + 1 < args.length) {
               parsed.tokenFile = Path.of(args[++i]);
            } else if ("--holyrics-home".equals(arg) && i + 1 < args.length) {
               parsed.holyricsHome = Path.of(args[++i]);
            }
         }
         return parsed;
      }
   }
}
