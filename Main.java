import java.io.BufferedReader;
import java.io.FileInputStream;
import java.util.Scanner;
import java.io.InputStreamReader;
import java.util.ArrayList;
import java.net.HttpURLConnection;
import java.net.URL;

public class Main {
    static String apiKey = "320d5e38afmsh4303ea9c1b8f26dp1ba115jsnee49a5333256";

    public static void main(String args[]) {
        Scanner userInput = new Scanner(System.in);
        System.out.print("Enter filename: ");
        ArrayList<String> urls = readFile(userInput.nextLine());
        userInput.close();
        getUrlInfo(urls);
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

    private static ArrayList<String> readFile(String filename) {

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
}