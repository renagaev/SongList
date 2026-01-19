package com.limagiran.googledrive;

import java.io.Serializable;
import java.util.Objects;

public class SyncBytesItem implements Serializable, Comparable<SyncBytesItem> {
   private static final long serialVersionUID = 11111111L;
   public final String id;
   public final String title;
   private final byte[] bytes;
   public long modifiedTime;

   public SyncBytesItem(String id, String title, byte[] bytes, long modifiedTime) {
      this.id = id;
      this.title = title;
      this.bytes = bytes;
      this.modifiedTime = modifiedTime;
   }

   public String getId() {
      return this.id;
   }

   public String getTitle() {
      return this.title;
   }

   public byte[] getBytes() {
      return this.bytes == null ? new byte[0] : (byte[])this.bytes.clone();
   }

   public long getModifiedTime() {
      return this.modifiedTime;
   }

   public int compareTo(SyncBytesItem o) {
      return o == null ? 1 : this.id.compareTo(o.id);
   }

   public boolean equals(Object obj) {
      return obj instanceof SyncBytesItem ? Objects.equals(((SyncBytesItem)obj).id, this.id) : super.equals(obj);
   }

   public int hashCode() {
      return 159 + Objects.hashCode(this.id);
   }
}
