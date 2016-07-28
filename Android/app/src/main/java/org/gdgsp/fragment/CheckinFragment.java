package org.gdgsp.fragment;

import android.app.Activity;
import android.content.SharedPreferences;
import android.content.pm.ActivityInfo;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.os.Bundle;
import android.preference.PreferenceManager;
import android.util.Base64;
import android.view.View;
import android.support.v4.app.Fragment;
import android.view.LayoutInflater;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.TextView;
import com.google.gson.Gson;
import com.google.gson.reflect.TypeToken;
import org.gdgsp.R;
import org.gdgsp.model.Person;
import java.lang.reflect.Type;

/**
 * Fragment onde o usuário realiza check-in.
 */
public class CheckinFragment extends Fragment {
	private Activity activity;
	private View view;

	@Override
	public void onAttach(Activity activity) {
		super.onAttach(activity);
		this.activity = getActivity();
	}

	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
	}

	@Override
	public View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		view = inflater.inflate(R.layout.fragment_checkin, container, false);

		SharedPreferences preferences = PreferenceManager.getDefaultSharedPreferences(activity);
		ImageView qr = (ImageView)view.findViewById(R.id.checkin_qrcode);

		byte[] qrBase64 = Base64.decode(preferences.getString("qr_code", ""), Base64.DEFAULT);
		Bitmap qrImage = BitmapFactory.decodeByteArray(qrBase64, 0, qrBase64.length);

		qr.setImageBitmap(qrImage);

		if(preferences.contains("member_profile")) {
			Gson gson = new Gson();

			Type datasetListType = new TypeToken<Person>() {}.getType();
			Person person = gson.fromJson(preferences.getString("member_profile", ""), datasetListType);

			TextView name = (TextView)view.findViewById(R.id.name);
			name.setText(person.getName());
		}
		
		activity.setRequestedOrientation(ActivityInfo.SCREEN_ORIENTATION_PORTRAIT);

		return view;
	}
}
