using System;

namespace EthereumWallet
{
    // Domain-specific exceptions for Wallet operations
    public class NegativeCreateException : ApplicationException { }
    public class NegativeDepositException : ApplicationException { }
    public class DivisionDepositException : ApplicationException { }
    public class TooMuchDepositException : ApplicationException { }

    public class NotEnoughFundsWithdrawException : ApplicationException { }
    public class NegativeWithdrawException : ApplicationException { }
    public class DivisionWithdrawException : ApplicationException { }
    public class TooMuchWithdrawException : ApplicationException { }

    public class NotEnoughFundsTransferException : ApplicationException { }
    public class NegativeTransferException : ApplicationException { }
    public class DivisionTransferException : ApplicationException { }
    public class TooMuchTransferException : ApplicationException { }
}
