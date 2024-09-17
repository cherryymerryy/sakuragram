using System;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using TdLib;

namespace CherryMerryGramDesktop.Views.Settings;

public partial class Profile : Page
{
    private static TdClient _client = App._client;
    private static TdApi.User _currentUser;
    private static TdApi.UserFullInfo _currentUserFullInfo;
    
    private int _profilePhotoFileId = 0;
    
    public Profile()
    {
        InitializeComponent();
        
        _currentUser = _client.GetMeAsync().Result;
        _currentUserFullInfo = _client.GetUserFullInfoAsync(userId: _currentUser.Id).Result;
        
        PersonPicture.DisplayName = _currentUser.FirstName + " " + _currentUser.LastName;
        
        TextBlockId.Text = $"ID: {_currentUser.Id}";
        TextBoxFirstName.Text = _currentUser.FirstName;
        TextBoxLastName.Text = _currentUser.LastName;
        TextBoxUsername.Text = _currentUser.Usernames.EditableUsername;
        TextBoxBio.Text = _currentUserFullInfo.Bio.Text;
        
        CardPhoneNumber.Description = $"Phone number: {_currentUser.PhoneNumber}";
        
        if (_currentUserFullInfo.Birthdate != null)
        {
            ButtonRemoveDateOfBirth.IsEnabled = true;
            DatePicker.MaxYear = new DateTimeOffset(new DateTime(year: 2024, month: 12, day: 31));
            DatePicker.MinYear = new DateTimeOffset(new DateTime(year: 1900, month: 1, day: 1));
            DatePicker.SelectedDate = new DateTimeOffset(
                new DateTime(
                    _currentUserFullInfo.Birthdate.Year, 
                    _currentUserFullInfo.Birthdate.Month, 
                    _currentUserFullInfo.Birthdate.Day
                ));
            CardDateOfBirth.Description = 
                $"Date of birth: {_currentUserFullInfo.Birthdate.Day}.{_currentUserFullInfo.Birthdate.Month}.{_currentUserFullInfo.Birthdate.Year}";
        }
        else
        {
            ButtonRemoveDateOfBirth.IsEnabled = false;
            CardDateOfBirth.Description = "";
        }
        
        UpdateProfilePhoto();
        
        _client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); };
    }

    private async Task ProcessUpdates(TdApi.Update update)
    {
        switch (update)
        {
            case TdApi.Update.UpdateFile updateFile:
            {
                if (updateFile.File.Id != _profilePhotoFileId) return;
                PersonPicture.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High,
                    () => PersonPicture.ProfilePicture = new BitmapImage(new Uri(updateFile.File.Local.Path)));
                break;
            }
        }
    }

    private void UpdateProfilePhoto()
    {
        if (_currentUser.ProfilePhoto == null) return;
        if (_currentUser.ProfilePhoto.Big.Local.Path != "")
        {
            try
            {
                PersonPicture.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High,
                    () => PersonPicture.ProfilePicture = new BitmapImage(new Uri(_currentUser.ProfilePhoto.Big.Local.Path)));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        else
        {
            _profilePhotoFileId = _currentUser.ProfilePhoto.Big.Id;
                
            var file = _client.ExecuteAsync(new TdApi.DownloadFile
            {
                FileId = _profilePhotoFileId,
                Priority = 1
            }).Result;
        }
    }

    private void UpdateBirthDate()
    {
        _client.ExecuteAsync(new TdApi.SetBirthdate
        {
            Birthdate = new TdApi.Birthdate
            {
                Day = DatePicker.Date.Day,
                Month = DatePicker.Date.Month,
                Year = DatePicker.Date.Year
            }
        });
        ButtonRemoveDateOfBirth.IsEnabled = true;
        CardDateOfBirth.Description = 
            $"Date of birth: {DatePicker.Date.Day}.{DatePicker.Date.Month}.{DatePicker.Date.Year}";
    }
    
    private void ButtonLogOut_OnClick(object sender, RoutedEventArgs e)
    {
        _client.ExecuteAsync(new TdApi.LogOut());
        _client.CloseAsync();
        Application.Current.Exit();
    }

    private void ButtonSave_OnClick(object sender, RoutedEventArgs e)
    {
        if (TextBoxFirstName.Text != _currentUser.FirstName || TextBoxLastName.Text != _currentUser.LastName)
        {
            _client.ExecuteAsync(new TdApi.SetName
            {
                FirstName = TextBoxFirstName.Text,
                LastName = TextBoxLastName.Text
            });
        }
        if (TextBoxUsername.Text != _currentUser.Usernames.EditableUsername)
        {
            _client.ExecuteAsync(new TdApi.SetUsername
            {
                Username = TextBoxUsername.Text
            });
        }
        if (TextBoxBio.Text != _currentUserFullInfo.Bio.Text)
        {
            _client.ExecuteAsync(new TdApi.SetBio
            {
                Bio = TextBoxBio.Text
            });
        }

        if (_currentUserFullInfo.Birthdate != null)
        {
            if (DatePicker.Date.Day != _currentUserFullInfo.Birthdate.Day ||
                DatePicker.Date.Month != _currentUserFullInfo.Birthdate.Month ||
                DatePicker.Date.Year != _currentUserFullInfo.Birthdate.Year)
            {
                UpdateBirthDate();
            }
        }
        else
        {
            UpdateBirthDate();
        }
    }

    private void ButtonRemoveDateOfBirth_OnClick(object sender, RoutedEventArgs e)
    {
        if (_currentUserFullInfo.Birthdate == null) return;
        _client.ExecuteAsync(new TdApi.SetBirthdate
        {
            Birthdate = null
        });
        ButtonRemoveDateOfBirth.IsEnabled = false;
        DatePicker.SelectedDate = null;
    }
}