package com.limagiran.holyrics.model;

import java.io.Serializable;

public class DeletedItem implements Serializable {
   private static final long serialVersionUID = 1L;
   private boolean deleted;
   private long modifiedTime;

   public boolean isDeleted() {
      return this.deleted;
   }

   public long getModifiedTime() {
      return this.modifiedTime;
   }
}
