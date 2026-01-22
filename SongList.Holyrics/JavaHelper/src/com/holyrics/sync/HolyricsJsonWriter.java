package com.holyrics.sync;

import com.limagiran.holyrics.model.DeletedItem;
import java.util.Collection;
import java.util.List;
import java.util.Map;

final class HolyricsJsonWriter {
   private HolyricsJsonWriter() {
   }

   static String songsToJson(Collection<HolyricsSyncHelper.SongData> songs) {
      StringBuilder sb = new StringBuilder(1024);
      sb.append("[");
      boolean first = true;
      for (HolyricsSyncHelper.SongData song : songs) {
         if (!first) {
            sb.append(',');
         }
         first = false;
         appendSongJson(sb, song);
      }
      sb.append("]");
      return sb.toString();
   }

   static String deletedToJson(Map<String, DeletedItem> deleted) {
      StringBuilder sb = new StringBuilder(256);
      sb.append("{\"deleted\":[");
      boolean first = true;
      for (Map.Entry<String, DeletedItem> entry : deleted.entrySet()) {
         DeletedItem item = entry.getValue();
         if (item == null || !item.isDeleted()) {
            continue;
         }
         if (!first) {
            sb.append(',');
         }
         first = false;
         sb.append("{\"id\":\"").append(jsonEscape(entry.getKey())).append("\",");
         sb.append("\"modifiedTime\":").append(item.getModifiedTime()).append('}');
      }
      sb.append("]}");
      return sb.toString();
   }

   static String applyResultToJson(String name, String customMd5, byte[] bytes) {
      StringBuilder sb = new StringBuilder(512);
      sb.append("{\"name\":");
      appendJsonString(sb, name);
      sb.append(",\"custom_md5\":");
      appendJsonString(sb, customMd5);
      sb.append(",\"bytes_base64\":\"")
            .append(java.util.Base64.getEncoder().encodeToString(bytes))
            .append("\"}");
      return sb.toString();
   }

   private static void appendSongJson(StringBuilder sb, HolyricsSyncHelper.SongData song) {
      sb.append("{\"syncId\":");
      appendJsonString(sb, song.syncId);
      sb.append(",\"id\":").append(song.id);
      sb.append(",\"title\":");
      appendJsonString(sb, song.title);
      sb.append(",\"artist\":");
      appendJsonString(sb, song.artist);
      sb.append(",\"lyrics\":");
      appendJsonString(sb, song.lyrics);
      sb.append(",\"order\":");
      appendJsonStringList(sb, song.order);
      sb.append(",\"author\":");
      appendJsonString(sb, song.author);
      sb.append(",\"note\":");
      appendJsonString(sb, song.note);
      sb.append(",\"language\":");
      appendJsonString(sb, song.language);
      sb.append(",\"formatting\":");
      appendJsonString(sb, song.formatting);
      sb.append(",\"set\":").append(song.set);
      sb.append(",\"titleToSearch\":");
      appendJsonString(sb, song.titleToSearch);
      sb.append(",\"artistToSearch\":");
      appendJsonString(sb, song.artistToSearch);
      sb.append(",\"lyricsToSearch\":");
      appendJsonString(sb, song.lyricsToSearch);
      sb.append(",\"noteToSearch\":");
      appendJsonString(sb, song.noteToSearch);
      sb.append(",\"lyricsHTML\":");
      appendJsonString(sb, song.lyricsHTML);
      sb.append(",\"titleHTML\":");
      appendJsonString(sb, song.titleHTML);
      sb.append(",\"artistHTML\":");
      appendJsonString(sb, song.artistHTML);
      sb.append(",\"src\":");
      appendJsonString(sb, song.src);
      sb.append(",\"numIni\":");
      appendJsonInteger(sb, song.numIni);
      sb.append(",\"translations\":");
      appendJsonStringMatrix(sb, song.translations);
      sb.append(",\"archived\":").append(song.archived);
      sb.append(",\"modifiedTime\":").append(song.modifiedTime);
      sb.append(",\"itemModifiedTime\":").append(song.itemModifiedTime);
      sb.append(",\"midi\":");
      appendJsonString(sb, song.midi);
      sb.append(",\"strParams\":");
      appendJsonStringArray(sb, song.strParams);
      sb.append(",\"version\":").append(song.version);
      sb.append('}');
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

   private static void appendJsonString(StringBuilder sb, String value) {
      if (value == null) {
         sb.append("null");
         return;
      }
      sb.append('"').append(jsonEscape(value)).append('"');
   }

   private static void appendJsonStringList(StringBuilder sb, List values) {
      if (values == null) {
         sb.append("null");
         return;
      }
      sb.append('[');
      boolean first = true;
      for (Object item : values) {
         if (!first) {
            sb.append(',');
         }
         first = false;
         if (item == null) {
            sb.append("null");
            continue;
         }
         appendJsonString(sb, String.valueOf(item));
      }
      sb.append(']');
   }

   private static void appendJsonStringArray(StringBuilder sb, String[] values) {
      if (values == null) {
         sb.append("null");
         return;
      }
      sb.append('[');
      for (int i = 0; i < values.length; i++) {
         if (i > 0) {
            sb.append(',');
         }
         appendJsonString(sb, values[i]);
      }
      sb.append(']');
   }

   private static void appendJsonStringMatrix(StringBuilder sb, String[][] values) {
      if (values == null) {
         sb.append("null");
         return;
      }
      sb.append('[');
      for (int i = 0; i < values.length; i++) {
         if (i > 0) {
            sb.append(',');
         }
         String[] row = values[i];
         if (row == null) {
            sb.append("null");
         } else {
            appendJsonStringArray(sb, row);
         }
      }
      sb.append(']');
   }

   private static void appendJsonInteger(StringBuilder sb, Integer value) {
      if (value == null) {
         sb.append("null");
      } else {
         sb.append(value.intValue());
      }
   }
}
