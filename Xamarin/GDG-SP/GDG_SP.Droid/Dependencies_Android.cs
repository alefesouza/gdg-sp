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

using Android.Content;
using Android.Content.PM;
using Android.OS;
using Com.OneSignal;
using GDG_SP.Droid;

[assembly: Xamarin.Forms.Dependency(typeof(Dependencies_Android))]
namespace GDG_SP.Droid
{
	public class Dependencies_Android : IDependencies
	{
		public Dependencies_Android()
		{
		}

        public string GetAppVersion()
        {
            return MainActivity.c.PackageManager.GetPackageInfo(MainActivity.c.PackageName, 0).VersionName;
        }

        public string GetOSVersion()
        {
            return Build.VERSION.SdkInt.ToString();
        }

        public void SendOneSignalTag(string key, string value)
		{
            OneSignal.SendTag(key, value);
		}
	}
}
