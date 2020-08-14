import java.io.FileInputStream;
import java.util.Scanner;
import java.io.RandomAccessFile;
import java.util.ArrayList;
import java.net.HttpURLConnection;
import java.net.URL;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.io.InputStream;
import java.io.FileOutputStream;
import java.io.File;
import java.util.regex.Pattern;

import org.farng.mp3.id3.ID3v2_3;

import java.io.IOException;
import java.io.ByteArrayOutputStream;
import java.util.regex.Matcher;
import java.util.List;

// http://javamusictag.sourceforge.net/docs.htm

public class Main {
    static String apiKey = "320d5e38afmsh4303ea9c1b8f26dp1ba115jsnee49a5333256";

    private static final Pattern VID_ID_PATTERN = Pattern.compile("(?<=v\\=|youtu\\.be\\/)\\w+");
    private static final Pattern MP3_URL_PATTERN = Pattern
            // .compile("(?<=href=\\\")https{0,1}:\\/\\/s02.ytapivmp3\\.com\s*");
            // .compile("(?<=href=\\\")https{0,1}\\:\\/\\/(\\w|\\d){3}\\.ytapivmp3\\.com.+\\.mp3(?=\\\")");
            .compile("href=\"(.*?)\"");

    public static void main(String args[]) {
        Scanner userInput = new Scanner(System.in);
        System.out.print("Enter filename: ");
        ArrayList<String> urls = getUrlsFromFile(userInput.nextLine());
        userInput.close();

        for (int i = 0; i < urls.size(); i++) {
            try {
                String[] songInfo = getSongInfo(urls.get(i));
                String id = getID(songInfo[0]);
                String converter = loadConverter(id);
                String url = getMp3Url(converter);
                Song song = new Song();
                song.songTitle = songInfo[1];
                song.artist = songInfo[2];
                if (songInfo.length > 3) {
                    song.albumTitle = songInfo[3];
                }
                // Downloads the song into the current directory
                downloadStreamData(url, song.songTitle + ".mp3");

                // Move the folder into the directory of the album and then updates the song
                // file information
                updateInfo(sortSong(song), song);

            } catch (Exception e) {
                System.out.println("Failed to download song.");
            }
        }
    }

    private static String[] getSongInfo(String info) {
        return info.split("--");
    }

    private static String sortSong(Song song) {
        String currentDir = System.getProperty("user.dir");
        String albumDir = currentDir + "\\" + song.albumTitle;
        String songSource = currentDir + "\\" + song.songTitle + ".mp3";
        if (song.albumTitle == null) {
            return songSource;
        }
        String songDest = albumDir + "\\" + song.songTitle + ".mp3";
        File newDir = new File(albumDir);
        if (!newDir.exists()) {
            try {
                newDir.mkdir();
                moveFile(Paths.get(songSource), Paths.get(songDest));
            } catch (Exception e) {
                System.out.println("Failed to create new album folder: " + e);
            }
        } else {
            moveFile(Paths.get(songSource), Paths.get(songDest));
        }
        return songDest;
    }

    /**
     * Moves the file to a new destination
     * 
     * @param source
     * @param destination
     */
    private static void moveFile(Path source, Path destination) {
        try {
            Path fileMoved = Files.move(source, destination);
            if (fileMoved == null) {
                System.out.println("Failed to move file to folder.");
            }
        } catch (Exception e) {
            System.out.println("Failed to move file to folder: " + e);
        }
    }

    /**
     * The url pattern needs to be reworked so it only recognizes the important
     * download link. Right now it gets all the href tags and only takes the 5th one
     * (which is the download link)
     * 
     * @param html
     * @return
     */
    private static String getMp3Url(String html) {
        Matcher linkMatcher = MP3_URL_PATTERN.matcher(html);
        List<String> allMatches = new ArrayList<String>();
        while (linkMatcher.find()) {
            allMatches.add(linkMatcher.group());
        }
        String link = allMatches.get(5);
        link = link.substring(6, link.length());
        link = link.substring(0, link.length() - 1);
        return link;
    }

    ///////////////////////////// WORKS ///////////////////////////////////////

    /**
     * Updates the information of the mp3 file based on the song info retrieved from
     * API
     * 
     * @param songFileSource
     * @param songInfo
     */
    private static void updateInfo(String songFileSource, Song songInfo) {
        try {
            RandomAccessFile file = new RandomAccessFile(songFileSource, "rw");
            ID3v2_3 tag = new ID3v2_3(file);
            tag.setSongTitle(songInfo.songTitle);
            tag.setLeadArtist(songInfo.artist);
            if (songInfo.albumTitle != null) {
                tag.setAlbumTitle(songInfo.albumTitle);
            }

            // Save changes to mp3 file
            tag.write(file);
        } catch (Exception e) {
            System.out.println("Failed to update song information.");
        }
    }

    /**
     * Gets all the url links from file input
     * 
     * @param filename
     * @return
     */
    private static ArrayList<String> getUrlsFromFile(String filename) {
        ArrayList<String> urls = new ArrayList<String>();
        try {
            FileInputStream fileInputStream = new FileInputStream(filename);
            Scanner fileScanner = new Scanner(fileInputStream);
            while (fileScanner.hasNextLine()) {
                urls.add(fileScanner.nextLine());
            }
            fileInputStream.close();
            fileScanner.close();
        } catch (Exception e) {
            System.out.println("Failed to read file.");
        }
        return urls;
    }

    /**
     * Gets the YouTube's ID of the url
     * 
     * @param youtubeUrl
     * @return
     */
    private static String getID(String youtubeUrl) {
        Matcher m = VID_ID_PATTERN.matcher(youtubeUrl);
        if (!m.find()) {
            throw new IllegalArgumentException("Invalid YouTube URL.");
        }
        return m.group();
    }

    /**
     * The commented code can be used to check the status of the download
     * 
     * @param url
     * @param fileName
     * @throws Exception
     */
    private static void downloadStreamData(String url, String fileName) throws Exception {
        URL tU = new URL(url);
        HttpURLConnection conn = (HttpURLConnection) tU.openConnection();
        InputStream ins = conn.getInputStream();
        FileOutputStream fout = new FileOutputStream(new File(fileName));
        byte[] outputByte = new byte[4096];
        int bytesRead;
        // int length = conn.getContentLength();
        // int read = 0;
        while ((bytesRead = ins.read(outputByte, 0, 4096)) != -1) {
            // read += bytesRead;
            // System.out.println(read + " out of " + length);
            fout.write(outputByte, 0, bytesRead);
        }
        fout.flush();
        fout.close();
    }

    /**
     * Uses 320youtube api to convert the youtube video
     * 
     * @param id
     * @return
     * @throws IOException
     */
    private static String loadConverter(String id) throws IOException {
        String url = "https://www.320youtube.com/watch?v=" + id;
        URL url2 = new URL(url);
        ByteArrayOutputStream baos = new ByteArrayOutputStream();
        InputStream is = url2.openStream();
        byte[] byteChunk = new byte[2500];
        int n;
        while ((n = is.read(byteChunk)) > 0) {
            baos.write(byteChunk, 0, n);
        }
        is.close();
        baos.flush();
        baos.close();
        byte[] bytes = baos.toByteArray();
        return new String(bytes);
    }
}

class Song {
    public String songTitle;
    public String artist;
    public String albumTitle;
}