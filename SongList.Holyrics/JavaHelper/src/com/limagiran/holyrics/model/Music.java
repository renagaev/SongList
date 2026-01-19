package com.limagiran.holyrics.model;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.List;

public class Music implements Serializable {
   private static final long serialVersionUID = 12491L;
   private long id;
   private String artist;
   private String title;
   private String lyrics;
   private List order;
   private String note;
   private String language;
   private String formatting;
   private long set;
   private String titleToSearch;
   private String artistToSearch;
   private String lyricsToSearch;
   private String noteToSearch;
   private String lyricsHTML;
   private String titleHTML;
   private String artistHTML;
   private String author;
   private String src;
   private Integer numIni;
   private String[][] translations;
   private boolean archived;
   private long modifiedTime;
   private String midi;
   private String[] strParams;
   private long version;

   public Music() {
      this.order = new ArrayList();
      this.language = "";
      this.formatting = "";
      this.titleToSearch = null;
      this.artistToSearch = null;
      this.lyricsToSearch = null;
      this.noteToSearch = null;
      this.lyricsHTML = null;
      this.titleHTML = null;
      this.artistHTML = null;
      this.author = "";
      this.src = "";
      this.numIni = 0;
      this.translations = null;
      this.midi = null;
      this.version = 2L;
   }

   public long getId() {
      return this.id;
   }

   public void setId(long id) {
      this.id = id;
   }

   public String getArtist() {
      return this.artist == null ? "" : this.artist;
   }

   public void setArtist(String artist) {
      this.artist = artist;
   }

   public String getTitle() {
      return this.title == null ? "" : this.title;
   }

   public void setTitle(String title) {
      this.title = title;
   }

   public String getLyrics() {
      return this.lyrics == null ? "" : this.lyrics;
   }

   public void setLyrics(String lyrics) {
      this.lyrics = lyrics;
   }

   public List getOrder() {
      return this.order;
   }

   public void setOrder(List order) {
      this.order = order;
   }

   public String getNote() {
      return this.note == null ? "" : this.note;
   }

   public void setNote(String note) {
      this.note = note;
   }

   public String getLanguage() {
      return this.language == null ? "" : this.language;
   }

   public void setLanguage(String language) {
      this.language = language;
   }

   public String getFormatting() {
      return this.formatting == null ? "" : this.formatting;
   }

   public void setFormatting(String formatting) {
      this.formatting = formatting;
   }

   public long getSet() {
      return this.set;
   }

   public void setSet(long set) {
      this.set = set;
   }

   public String getTitleToSearch() {
      return this.titleToSearch;
   }

   public void setTitleToSearch(String titleToSearch) {
      this.titleToSearch = titleToSearch;
   }

   public String getArtistToSearch() {
      return this.artistToSearch;
   }

   public void setArtistToSearch(String artistToSearch) {
      this.artistToSearch = artistToSearch;
   }

   public String getLyricsToSearch() {
      return this.lyricsToSearch;
   }

   public void setLyricsToSearch(String lyricsToSearch) {
      this.lyricsToSearch = lyricsToSearch;
   }

   public String getNoteToSearch() {
      return this.noteToSearch;
   }

   public void setNoteToSearch(String noteToSearch) {
      this.noteToSearch = noteToSearch;
   }

   public String getLyricsHTML() {
      return this.lyricsHTML;
   }

   public void setLyricsHTML(String lyricsHTML) {
      this.lyricsHTML = lyricsHTML;
   }

   public String getTitleHTML() {
      return this.titleHTML;
   }

   public void setTitleHTML(String titleHTML) {
      this.titleHTML = titleHTML;
   }

   public String getArtistHTML() {
      return this.artistHTML;
   }

   public void setArtistHTML(String artistHTML) {
      this.artistHTML = artistHTML;
   }

   public String getAuthor() {
      return this.author == null ? "" : this.author;
   }

   public void setAuthor(String author) {
      this.author = author;
   }

   public String getSrc() {
      return this.src;
   }

   public void setSrc(String src) {
      this.src = src;
   }

   public Integer getNumIni() {
      return this.numIni;
   }

   public void setNumIni(Integer numIni) {
      this.numIni = numIni;
   }

   public String[][] getTranslations() {
      return this.translations;
   }

   public void setTranslations(String[][] translations) {
      this.translations = translations;
   }

   public boolean isArchived() {
      return this.archived;
   }

   public void setArchived(boolean archived) {
      this.archived = archived;
   }

   public long getModifiedTime() {
      return this.modifiedTime;
   }

   public void setModifiedTime(long modifiedTime) {
      this.modifiedTime = modifiedTime;
   }

   public String getMidi() {
      return this.midi;
   }

   public void setMidi(String midi) {
      this.midi = midi;
   }

   public String[] getStrParams() {
      return this.strParams;
   }

   public void setStrParams(String[] strParams) {
      this.strParams = strParams;
   }

   public long getVersion() {
      return this.version;
   }

   public void setVersion(long version) {
      this.version = version;
   }
}
