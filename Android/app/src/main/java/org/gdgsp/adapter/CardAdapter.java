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
import android.view.View.OnLongClickListener;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.RelativeLayout;
import android.widget.TextView;
import com.koushikdutta.ion.Ion;
import java.util.List;
import org.gdgsp.R;
import org.gdgsp.model.Event;
import org.gdgsp.other.Other;
import org.gdgsp.activity.*;

/**
 * Adapter dos cards de eventos exibidos na MainActivity.
 */
public class CardAdapter extends RecyclerView.Adapter <RecyclerView.ViewHolder> {
	private Context context;
	private List<Event> events;
	private boolean isPast;

	public CardAdapter(Context context, List <Event> events, boolean isPast) {
		this.context = context;
		this.events = events;
		this.isPast = isPast;
	}

	@Override
	public RecyclerView.ViewHolder onCreateViewHolder(ViewGroup parent, 	int viewType) {
		View item = LayoutInflater.from(parent.getContext())
				.inflate(R.layout.card_event, null);

		ItemViewHolder viewHolder = new ItemViewHolder(item);
		return viewHolder;
	}

	public void remove(int position) {
		events.remove(position);
		notifyItemRemoved(position);
	}

	public void add(Event card, int position) {
		events.add(card);
		notifyItemInserted(position);
	}

	@Override
	public void onBindViewHolder(final RecyclerView.ViewHolder viewHolder, final int position) {
		if (viewHolder instanceof ItemViewHolder) {
			final ItemViewHolder holder = (ItemViewHolder)viewHolder;

			final String name = events.get(position).getName();
			final String url = events.get(position).getLink();

			holder.title.setText(name);

			Ion.with(context).load(events.get(position).getImage()).intoImageView(holder.image);

			holder.date.setText(events.get(position).getStart());
			holder.location.setText(events.get(position).getPlace());

			holder.content.setOnClickListener(new OnClickListener() {
				public void onClick(View v) {
					MainActivity main = (MainActivity)context;
					main.openEvent(main, events.get(position), isPast);
				}
			});

			holder.content.setOnLongClickListener(new OnLongClickListener() {
				@Override
				public boolean onLongClick(View p1) {
					Other.cardDialog(context, name, url);
					return false;
				}
			});
		}
	}

	public class ItemViewHolder extends RecyclerView.ViewHolder {

		public TextView title;
		public ImageView image;
		public TextView date;
		public TextView location;
		public RelativeLayout content;

		public ItemViewHolder(View item) {
			super(item);
			title = (TextView)item.findViewById(R.id.title);
			image = (ImageView)item.findViewById(R.id.image);
			date = (TextView)item.findViewById(R.id.date);
			location = (TextView)item.findViewById(R.id.location);
			content = (RelativeLayout)item.findViewById(R.id.content);
		}
	}

	@Override
	public int getItemCount() {
		return events.size();
	}
}
