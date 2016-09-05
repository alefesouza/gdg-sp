/*
 * Copyright (C) 2016 Alefe Souza <http://alefesouza.com>
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

package org.gdgsp.adapter;

import android.content.Context;
import android.support.v7.widget.RecyclerView;
import android.view.LayoutInflater;
import android.view.View;
import android.view.View.OnClickListener;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.RelativeLayout;
import android.widget.TextView;

import com.koushikdutta.ion.Ion;

import org.gdgsp.R;
import org.gdgsp.model.Tweet;
import org.gdgsp.other.Other;

import java.util.List;

/**
 * Adapter dos cards de tweets exibidos no TweetsFragment.
 */
public class TweetAdapter extends RecyclerView.Adapter <RecyclerView.ViewHolder> {
    private Context context;
    private List<Tweet> tweets;

    public TweetAdapter(Context context, List <Tweet> tweets) {
        this.context = context;
        this.tweets = tweets;
    }

    @Override
    public RecyclerView.ViewHolder onCreateViewHolder(ViewGroup parent, 	int viewType) {
        View item = LayoutInflater.from(parent.getContext())
			.inflate(R.layout.card_tweet, null);

        ItemViewHolder viewHolder = new ItemViewHolder(item);
        return viewHolder;
    }

    public void remove(int position) {
        tweets.remove(position);
        notifyItemRemoved(position);
    }

    public void add(Tweet card, int position) {
        tweets.add(card);
        notifyItemInserted(position);
    }

    @Override
    public void onBindViewHolder(final RecyclerView.ViewHolder holderr, final int position) {
        if (holderr instanceof ItemViewHolder) {
            ItemViewHolder holder = (ItemViewHolder)holderr;

            holder.user_name.setText(tweets.get(position).getUser_name());
            holder.screen_name.setText(tweets.get(position).getScreen_name());
            holder.date.setText(tweets.get(position).getDate());
            holder.text.setText(tweets.get(position).getText());

            Ion.with (context)
				.load(tweets.get(position).getPhoto())
				.withBitmap()
				.intoImageView(holder.image);

				holder.content.setOnClickListener(new OnClickListener() {
					public void onClick(View v) {
						Other.openTwitterApp(context, tweets.get(position).getLink());
					}
				});
        }
    }

    public class ItemViewHolder extends RecyclerView.ViewHolder {
        public TextView user_name, screen_name, date, text;
        public ImageView image;
        public RelativeLayout content;

        public ItemViewHolder(View item) {
            super(item);
            user_name = (TextView)item.findViewById(R.id.user_name);
            screen_name = (TextView)item.findViewById(R.id.screen_name);
            date = (TextView)item.findViewById(R.id.date);
            text = (TextView)item.findViewById(R.id.text);
            image = (ImageView)item.findViewById(R.id.profileImage);
            content = (RelativeLayout)item.findViewById(R.id.content);
        }
    }

    @Override
    public int getItemCount() {
        return tweets.size();
    }
}
