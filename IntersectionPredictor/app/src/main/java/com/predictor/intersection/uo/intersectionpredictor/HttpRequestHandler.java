package com.predictor.intersection.uo.intersectionpredictor;

import android.os.AsyncTask;
import android.util.Log;
import android.widget.TextView;

import java.net.*;
import java.io.*;

public class HttpRequestHandler extends AsyncTask<String, Void, String> {

    TextView tv;

    public void setTextView(TextView v) {
        tv = v;
    }
    @Override
    protected String doInBackground(String... strings) {
        String result = "";

        try {
            String inputLine;

            URL url = new URL(strings[0]);
            HttpURLConnection connection =(HttpURLConnection)
                    url.openConnection();

            //Set methods and timeouts
            connection.setRequestMethod("GET");

            //Connect to our url
            connection.connect();
            //Create a new InputStreamReader
            InputStreamReader streamReader = new InputStreamReader(connection.getInputStream());
            //Create a new buffered reader and String Builder
            BufferedReader reader = new BufferedReader(streamReader);
            StringBuilder stringBuilder = new StringBuilder();
            //Check if the line we are reading is not null
            while((inputLine = reader.readLine()) != null){
                stringBuilder.append(inputLine);
            }
            //Close our InputStream and Buffered reader
            reader.close();
            streamReader.close();
            //Set our result equal to our stringBuilder
            result = stringBuilder.toString();
            Log.d("http_res", result);

        }
        catch (Exception e) {
            Log.d("http", e.getMessage());
        }
        return result;
    }

    @Override
    protected void onPostExecute(String result) {
        tv.setText(result);
    }
}
