package com.limagiran.googledrive;

import java.io.Serializable;
import java.math.BigInteger;
import java.security.MessageDigest;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.UUID;

public class SyncBytesList implements Serializable {
   private static final long serialVersionUID = 11111111L;
   private final Map items = new HashMap();
   private transient String md5;
   private transient Long bytesLength;

   public void invalidateCache() {
      this.md5 = null;
      this.bytesLength = null;
   }

   public List getItems() {
      synchronized(this) {
         return new ArrayList(this.items.values());
      }
   }

   public Map getItemsMap() {
      return this.items;
   }

   public int getSize() {
      synchronized(this) {
         return this.items.size();
      }
   }

   public String getMD5() {
      String _md5 = this.md5;
      if (_md5 == null) {
         try {
            _md5 = this.createMD5();
         } catch (Exception var3) {
            _md5 = UUID.randomUUID().toString().replace("-", "");
         }

         this.md5 = _md5;
      }

      return _md5;
   }

   private String createMD5() throws Exception {
      MessageDigest enc = MessageDigest.getInstance("MD5");
      synchronized(this) {
         this.items.values().stream().sorted((o1, o2) -> ((SyncBytesItem)o1).id.compareTo(((SyncBytesItem)o2).id)).forEach((itemObj) -> {
            SyncBytesItem item = (SyncBytesItem)itemObj;
            String id = item.id;
            byte[] bytes = new byte[id.length() + 8];

            for(int i = 0; i < id.length(); ++i) {
               bytes[i] = (byte)id.charAt(i);
            }

            long l = item.getModifiedTime();

            for(int i = id.length(); i < bytes.length; ++i) {
               bytes[i] = (byte)((int)(l & 255L));
               l >>= 8;
            }

            enc.update(bytes, 0, bytes.length);
         });
      }

      String _md5;
      for(_md5 = (new BigInteger(1, enc.digest())).toString(16); _md5.length() < 32; _md5 = "0" + _md5) {
      }

      return _md5;
   }
}
