import java.io.BufferedReader;
import java.io.FileInputStream;
import java.util.Scanner;
import java.io.InputStreamReader;
import java.io.RandomAccessFile;
import java.util.ArrayList;
import java.net.HttpURLConnection;
import java.net.URL;
import java.io.InputStream;
import java.io.FileOutputStream;
import java.io.File;
import java.util.regex.Pattern;

import org.farng.mp3.id3.ID3v2_3;

import java.io.IOException;
import java.io.ByteArrayOutputStream;
import java.util.regex.Matcher;
import java.util.List;

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
        // For each url loop the following
        for (String url : urls) {
            try {
                String id = getID(url);
                String converter = loadConverter(id);
                String mp3url = getMP3URL(converter); // Maybe get the song title from here
                downloadStreamData(mp3url, "test.mp3");
                // Hit the api to get song information
                // updateInfo("C:\\Users\\joshv\\Desktop\\Github\\YoutubeConverter\\test.mp3");
            } catch (Exception e) {
                System.out.println("Failed");
            }
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
    private static String getMP3URL(String html) {
        Matcher m = MP3_URL_PATTERN.matcher(html);
        List<String> allMatches = new ArrayList<String>();
        while (m.find()) {
            allMatches.add(m.group());
        }
        String link = allMatches.get(5);
        link = link.substring(6, link.length());
        link = link.substring(0, link.length() - 1);
        return link;
    }

    // Hits the
    private static void getUrlInfo(ArrayList<String> urls) {

        // https://www.baeldung.com/java-http-request
        try {
            URL url = new URL("http://example.com");
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
            System.out.println(content);
        } catch (Exception e) {

        }

        // OkHttpClient client = new OkHttpClient();

        // Request request = new Request.Builder()
        // .url("https://shazam.p.rapidapi.com/songs/get-details?locale=en-US&key=40333609").get()
        // .addHeader("x-rapidapi-host", "shazam.p.rapidapi.com").addHeader().build();

        // Response response = client.newCall(request).execute();
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