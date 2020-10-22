# YoutubeConverter
Gets the information of a song from a Youtube url. It then downloads the file as an MP3 file.

## API
The application uses the Shazam API which queries for the song information. It retrieves the official song title, artist, and album of the song. 

## How it works
Once you have obtained your API, paste the youtube url of a song into the field and click convert to start the process. Initially it will query for the song information which will be displayed to the user to be verified. Once the song information is verfied, press download to start the process.
Once the file has been downloaded, the application then edits its details with the verified information retrieved.
If the song information is incorrect, the user can manually enter the correct details instead.
To convert more than one url, save the urls into a text file, each separated by a new line. 
Click import on the application and it will automatically start the whole process. Do note that using this functionality does not allow for verification of information that is being written to the file so use with discretion.