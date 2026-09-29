using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using SmartFinanceManager.Helpers;
using SmartFinanceManager.Models;
using SmartFinanceManager.Services;

namespace SmartFinanceManager.ViewModels
{
    public class SubscriptionBenefitItem
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class SubscriptionViewModel : ViewModelBase
    {
        private readonly SubscriptionService _subscriptionService;
        private readonly int _userId;

        private bool _isFlipped;
        private string _email = string.Empty;
        private string _transactionId = string.Empty;
        private string _errorMessage = string.Empty;
        private string _successMessage = string.Empty;
        private bool _isSuccess;
        private string _copyButtonText = "Copy";
        private SubscriptionStatus? _currentStatus;
        private string? _adminNote;

        public bool IsFlipped
        {
            get => _isFlipped;
            set
            {
                if (SetProperty(ref _isFlipped, value))
                {
                    OnPropertyChanged(nameof(IsFrontVisible));
                    OnPropertyChanged(nameof(IsBackVisible));
                }
            }
        }

        public bool IsFrontVisible => !IsFlipped;
        public bool IsBackVisible => IsFlipped;

        public string PlanTitle => "Office Finance Management";
        public string PlanPrice => "100 BDT";
        public string PlanFeeLabel => "ONE-TIME FEE";
        public string RecipientNumber => AdminConfig.BkashRecipientNumber;

        public ObservableCollection<SubscriptionBenefitItem> Benefits { get; } = new();

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string TransactionId
        {
            get => _transactionId;
            set => SetProperty(ref _transactionId, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public string SuccessMessage
        {
            get => _successMessage;
            set => SetProperty(ref _successMessage, value);
        }

        public bool IsSuccess
        {
            get => _isSuccess;
            set => SetProperty(ref _isSuccess, value);
        }

        public string CopyButtonText
        {
            get => _copyButtonText;
            set => SetProperty(ref _copyButtonText, value);
        }

        public SubscriptionStatus? CurrentStatus
        {
            get => _currentStatus;
            set
            {
                if (SetProperty(ref _currentStatus, value))
                {
                    OnPropertyChanged(nameof(HasPendingStatus));
                    OnPropertyChanged(nameof(HasApprovedStatus));
                    OnPropertyChanged(nameof(HasRejectedStatus));
                }
            }
        }

        public bool HasPendingStatus => CurrentStatus == SubscriptionStatus.Pending;
        public bool HasApprovedStatus => CurrentStatus == SubscriptionStatus.Approved;
        public bool HasRejectedStatus => CurrentStatus == SubscriptionStatus.Rejected;

        public string? AdminNote
        {
            get => _adminNote;
            set => SetProperty(ref _adminNote, value);
        }

        public ICommand FlipToCheckoutCommand { get; }
        public ICommand FlipToFrontCommand { get; }
        public ICommand CopyRecipientNumberCommand { get; }
        public ICommand VerifyPaymentCommand { get; }
        public ICommand CloseCommand { get; }

        public event Action? RequestClose;
        public event Action? OnSubscriptionSubmitted;

        public SubscriptionViewModel(SubscriptionService? subscriptionService = null, User? user = null)
        {
            _subscriptionService = subscriptionService ?? new SubscriptionService();
            
            var targetUser = user ?? AuthService.CurrentUser;
            _userId = targetUser?.Id ?? 0;
            Email = targetUser?.Email ?? string.Empty;

            FlipToCheckoutCommand = new RelayCommand(() => IsFlipped = true);
            FlipToFrontCommand = new RelayCommand(() => IsFlipped = false);
            CopyRecipientNumberCommand = new RelayCommand(ExecuteCopyRecipientNumber);
            VerifyPaymentCommand = new RelayCommand(ExecuteVerifyPayment);
            CloseCommand = new RelayCommand(() => RequestClose?.Invoke());

            LoadBenefits();
            RefreshStatus();
        }

        private void LoadBenefits()
        {
            Benefits.Clear();
            Benefits.Add(new SubscriptionBenefitItem { Title = "Office Finance Dashboard", Description = "Dedicated corporate revenue, expense, and cash flow tracking" });
            Benefits.Add(new SubscriptionBenefitItem { Title = "Business Transactions", Description = "Track client sales, contracts, payroll, software SaaS, and leases" });
            Benefits.Add(new SubscriptionBenefitItem { Title = "Office Budgets & Limits", Description = "Set and monitor monthly departmental spending targets" });
            Benefits.Add(new SubscriptionBenefitItem { Title = "Financial Reports & Analytics", Description = "In-depth corporate spending analytics and interactive charts" });
            Benefits.Add(new SubscriptionBenefitItem { Title = "Priority Data Security", Description = "Strict organization-level user and financial record isolation" });
        }

        public void RefreshStatus()
        {
            if (_userId > 0)
            {
                var latestSub = _subscriptionService.GetLatestUserSubscription(_userId);
                CurrentStatus = latestSub?.Status;
                AdminNote = latestSub?.AdminNote;

                if (CurrentStatus == SubscriptionStatus.Pending)
                {
                    SuccessMessage = "Your verification request is currently pending administrator approval.";
                    IsSuccess = true;
                }
                else if (CurrentStatus == SubscriptionStatus.Approved)
                {
                    SuccessMessage = "Your Office Finance subscription is Active and Approved!";
                    IsSuccess = true;
                }
            }
        }

        private void ExecuteCopyRecipientNumber()
        {
            try
            {
                Clipboard.SetText(RecipientNumber);
                CopyButtonText = "Copied!";
                Task.Delay(2000).ContinueWith(_ =>
                {
                    CopyButtonText = "Copy";
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch
            {
                CopyButtonText = "01903247467";
            }
        }

        private void ExecuteVerifyPayment()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            IsSuccess = false;

            if (string.IsNullOrWhiteSpace(TransactionId))
            {
                ErrorMessage = "Please enter the bKash Transaction ID from your confirmation SMS/App.";
                return;
            }

            if (_userId <= 0)
            {
                ErrorMessage = "Please create an account or log in before submitting payment verification.";
                return;
            }

            var (success, message, sub) = _subscriptionService.CreateSubscriptionRequest(
                _userId,
                Email,
                AdminConfig.OfficeSubscriptionPrice,
                TransactionId,
                "bKash"
            );

            if (success)
            {
                IsSuccess = true;
                SuccessMessage = message;
                CurrentStatus = SubscriptionStatus.Pending;
                OnSubscriptionSubmitted?.Invoke();
            }
            else
            {
                ErrorMessage = message;
            }
        }
    }
}
