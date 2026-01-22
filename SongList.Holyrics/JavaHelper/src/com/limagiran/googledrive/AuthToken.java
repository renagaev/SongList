package com.limagiran.googledrive;

import java.io.Serializable;

public class AuthToken implements Serializable {
   private static final long serialVersionUID = 1L;
   private String access_token;
   private String token_type;
   private long expires_in;
   private String refresh_token;
   private String scope;
   private long created_at;

   public String getAccess_token() {
      return this.access_token;
   }

   public String getToken_type() {
      return this.token_type;
   }

   public long getExpires_in() {
      return this.expires_in;
   }

   public String getRefresh_token() {
      return this.refresh_token;
   }

   public String getScope() {
      return this.scope;
   }

   public long getCreated_at() {
      return this.created_at;
   }
}
