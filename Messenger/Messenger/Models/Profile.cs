﻿using Mikodev.Network;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Messenger.Models
{
    /// <summary>
    /// 用户信息
    /// </summary>
    public class Profile : INotifyPropertyChanging, INotifyPropertyChanged
    {
        public static event PropertyChangedEventHandler InstancePropertyChanged;

        public static event PropertyChangingEventHandler InstancePropertyChanging;

        public event PropertyChangedEventHandler PropertyChanged;

        public event PropertyChangingEventHandler PropertyChanging;

        private void OnPropertyChange<T>(ref T source, T target, [CallerMemberName] string name = null)
        {
            var eva = new PropertyChangingEventArgs(name);
            PropertyChanging?.Invoke(this, eva);
            InstancePropertyChanging?.Invoke(this, eva);

            if (Equals(source, target))
                return;
            source = target;

            var evb = new PropertyChangedEventArgs(name);
            PropertyChanged?.Invoke(this, evb);
            InstancePropertyChanged?.Invoke(this, evb);
        }

        private readonly int _id;
        private int _hint = 0;
        private string _name = null;
        private string _text = null;
        private string _logo = null;

        public Profile() { }

        public Profile(int id) => _id = id;

        public bool IsGroups => _id < Links.Id;

        public int Id => _id;

        /// <summary>
        /// 未读消息计数
        /// </summary>
        public int Hint
        {
            get => _hint;
            set => OnPropertyChange(ref _hint, value);
        }

        public string Name
        {
            get => _name;
            set => OnPropertyChange(ref _name, value);
        }

        public string Text
        {
            get => _text;
            set => OnPropertyChange(ref _text, value);
        }

        public string Image
        {
            get => _logo;
            set => OnPropertyChange(ref _logo, value);
        }

        public Profile CopyFrom(Profile profile)
        {
            Name = profile._name;
            Text = profile._text;
            Image = profile._logo;
            return this;
        }
    }
}
