using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls.Map;

namespace Windows_Desktop
{
    public class MyMapItem
    {
        private Location _location;
        private string _title;
        private string _description;

        public MyMapItem(string title, double latitude, double longitude, string description = "")
        {
            this.Title = title;
            this.Location = new Location(latitude, longitude);
            this.Description = description;
        }

        public Location Location
        {
            get
            {
                return this._location;
            }
            set
            {
                this._location = value;
            }
        }

        public string Title
        {
            get
            {
                return this._title;
            }
            set
            {
                this._title = value;
            }
        }

        public string Description
        {
            get
            {
                return this._description;
            }
            set
            {
                this._description = value;
            }
        }
    }
}
