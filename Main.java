import java.io.BufferedReader;
import java.io.FileInputStream;
import java.util.Scanner;
import java.io.InputStreamReader;
import java.io.RandomAccessFile;
import java.util.ArrayList;
import java.util.Arrays;
import java.net.HttpURLConnection;
import java.net.URL;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.nio.file.StandardCopyOption;
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
    private static final Pattern SONG_TITLE = Pattern.compile("<title>((.|\\n)*?)<\\/title>");

    public static void main(String args[]) {
        Scanner userInput = new Scanner(System.in);
        System.out.print("Enter filename: ");
        ArrayList<String> urls = getUrlsFromFile(userInput.nextLine());
        userInput.close();

        for (int i = 0; i < urls.size(); i++) {
            try {
                String id = getID(urls.get(i));
                String converter = loadConverter(id);
                ArrayList<String> urlTitle = getMp3UrlAndTitle(converter);

                // Calls an api to get the song information (title, artist, album)
                Song songInfo = getSongInfo(urlTitle.get(0));

                // System.out.println("TITLE: " + songInfo.songTitle);
                // System.out.println("ARTIST: " + songInfo.artist);
                // System.out.println("ALBUM: " + songInfo.albumTitle);

                // Downloads the song into the current directory
                // downloadStreamData(urlTitle.get(1), songInfo.songTitle + ".mp3");

                // Move the folder into the directory of the album and then updates the song
                // file information
                // updateInfo(sortSong(songInfo), songInfo);

            } catch (Exception e) {
                System.out.println("Failed to download song.");
            }
        }
    }

    private static String sortSong(Song song) {
        String currentDir = System.getProperty("user.dir");
        String albumDir = currentDir + "\\" + song.albumTitle;
        String songSource = currentDir + "\\" + song.songTitle + ".mp3";
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

    // Hits the shazam api to get the song information
    private static Song getSongInfo(String keywords) {
        // https://www.baeldung.com/java-http-request
        String rootUrl = "https://shazam.p.rapidapi.com/search?locale=en-US&offset=0&limit=5&term=";
        try {
            URL url = new URL(rootUrl + extractKeywords(keywords));
            HttpURLConnection con = (HttpURLConnection) url.openConnection();
            con.setRequestMethod("GET");
            con.setRequestProperty("x-rapidapi-host", "shazam.p.rapidapi.com");
            con.setRequestProperty("x-rapidapi-key", apiKey);

            // int status = con.getResponseCode();
            BufferedReader bufferedReader = new BufferedReader(new InputStreamReader(con.getInputStream()));
            String inputLine;
            StringBuffer content = new StringBuffer();
            while ((inputLine = bufferedReader.readLine()) != null) {
                content.append(inputLine);
            }
            bufferedReader.close();
            con.disconnect();
            return filterSongInfo(content); // Process the result here -- filter and get
            // song information
        } catch (Exception e) {
            System.out.println("Failed to get song information.");
        }
        return null;
    }

    // Extract the keywords from the given title -- filter out "LYRICS"
    private static String extractKeywords(String keywords) {
        String key = keywords;
        ArrayList<String> strip = new ArrayList<String>(Arrays.asList("lyrics", "LYRICS", "Lyrics", "-"));
        for (String string : strip) {
            if (keywords.contains(string)) {
                key = key.replace(string, " ");
            }
        }
        key = key.replace(" ", "%20");
        System.out.println("KEY HERE: " + key);
        return key;
    }

    // Get the right song information here?
    private static Song filterSongInfo(StringBuffer content) {

        // Process the json result here (convert to class object for easier navigation?)
        System.out.println("CONTENT: " + content);
        return new Song();
    }

    /**
     * The url pattern needs to be reworked so it only recognizes the important
     * download link. Right now it gets all the href tags and only takes the 5th one
     * (which is the download link)
     * 
     * @param html
     * @return
     */
    private static ArrayList<String> getMp3UrlAndTitle(String html) {
        ArrayList<String> song = new ArrayList<>();
        Matcher linkMatcher = MP3_URL_PATTERN.matcher(html);
        Matcher titleMatcher = SONG_TITLE.matcher(html);
        List<String> allMatches = new ArrayList<String>();
        String title = "";
        while (linkMatcher.find()) {
            allMatches.add(linkMatcher.group());
        }
        if (titleMatcher.find()) {
            title = titleMatcher.group(1);
        }
        title = title.substring(0, title.length() - 12); // 12 characters to remove "| 320YouTube" at the end of the
                                                         // title string
        String link = allMatches.get(5);
        link = link.substring(6, link.length());
        link = link.substring(0, link.length() - 1);
        song.add(title);
        song.add(link);
        return song;
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
            tag.setAlbumTitle(songInfo.albumTitle);

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