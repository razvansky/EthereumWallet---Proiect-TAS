using System;
using System.Collections.Generic;
using System.Text;

namespace EthereumWallet
{
    public class Wallet
    {
        private float balance;
        private readonly List<Transaction> transactions = new List<Transaction>();

        public Wallet(float amount)
        {
            if (amount < 0)
            {
                throw new NegativeCreateException();
            }
            balance = amount;
            transactions.Add(Transaction.CreateInitial(amount));
        }

        public void Deposit(int amount)
        {
            if (amount < 0)
            {
                throw new NegativeDepositException();
            }
            else if (amount % 10 != 0)
            {
                throw new DivisionDepositException();
            }
            else if (amount > 50000)
            {
                throw new TooMuchDepositException();
            }
            balance += amount;
            transactions.Add(Transaction.Deposit(amount));
        }

        public void Withdraw(int amount)
        {
            if (amount > balance)
            {
                throw new NotEnoughFundsWithdrawException();
            }
            else if (amount < 0)
            {
                throw new NegativeWithdrawException();
            }
            else if (amount % 10 != 0)
            {
                throw new DivisionWithdrawException();
            }
            else if (amount > 50000)
            {
                throw new TooMuchWithdrawException();
            }
            balance -= amount;
            transactions.Add(Transaction.Withdraw(amount));
        }

        public void Transfer(Wallet destination, int amount)
        {
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            if (amount > balance)
            {
                throw new NotEnoughFundsTransferException();
            }
            else if (amount < 0)
            {
                throw new NegativeTransferException();
            }
            else if (amount % 10 != 0)
            {
                throw new DivisionTransferException();
            }
            else if (amount > 50000)
            {
                throw new TooMuchTransferException();
            }
            destination.Deposit(amount);
            Withdraw(amount);
            transactions.Add(Transaction.Transfer(amount, destination));
        }

        public void DepositBitcoin(float amountInBitcoin, ICurrencyConverterStub converter)
        {
            if (converter == null) throw new ArgumentNullException(nameof(converter));
            // Ensure negative BTC amounts are handled by wallet logic
            if (amountInBitcoin < 0)
            {
                throw new NegativeDepositException();
            }
            float amountInEthereum = converter.BitcoinToEthereum(amountInBitcoin);
            Deposit((int)amountInEthereum);
            transactions.Add(Transaction.DepositBitcoin(amountInBitcoin, amountInEthereum));
        }

        public void WithdrawBitcoin(float amountInBitcoin, ICurrencyConverterStub converter)
        {
            if (converter == null) throw new ArgumentNullException(nameof(converter));
            float amountInEthereum = converter.BitcoinToEthereum(amountInBitcoin);
            Withdraw((int)amountInEthereum);
            transactions.Add(Transaction.WithdrawBitcoin(amountInBitcoin, amountInEthereum));
        }

        public void TransferBitcoin(Wallet destination, float amountInBitcoin, ICurrencyConverterStub converter)
        {
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            if (converter == null) throw new ArgumentNullException(nameof(converter));
            // Ensure negative amounts are handled by wallet logic, not the converter
            if (amountInBitcoin < 0)
            {
                throw new NegativeTransferException();
            }
            float amountInEthereum = converter.BitcoinToEthereum(amountInBitcoin);
            Transfer(destination, (int)amountInEthereum);
            transactions.Add(Transaction.TransferBitcoin(amountInBitcoin, amountInEthereum, destination));
        }

        public float Balance => balance;

        public IReadOnlyList<Transaction> Transactions => transactions.AsReadOnly();
    }

    public class Transaction
    {
        public enum TransactionType
        {
            Initial,
            Deposit,
            Withdraw,
            Transfer,
            DepositBitcoin,
            WithdrawBitcoin,
            TransferBitcoin
        }

        public Guid Id { get; private set; } = Guid.NewGuid();
        public TransactionType Type { get; private set; }
        public int Amount { get; private set; }
        public DateTime Timestamp { get; private set; }
        public Wallet Destination { get; private set; }
        public float? AmountInBitcoin { get; private set; }
        public float? AmountInEthereum { get; private set; }

        private Transaction() { }

        public static Transaction CreateInitial(float balance)
        {
            return new Transaction { Type = TransactionType.Initial, Amount = (int)balance, Timestamp = DateTime.UtcNow };
        }

        public static Transaction Deposit(int amount)
        {
            return new Transaction { Type = TransactionType.Deposit, Amount = amount, Timestamp = DateTime.UtcNow };
        }

        public static Transaction Withdraw(int amount)
        {
            return new Transaction { Type = TransactionType.Withdraw, Amount = amount, Timestamp = DateTime.UtcNow };
        }

        public static Transaction Transfer(int amount, Wallet destination)
        {
            return new Transaction { Type = TransactionType.Transfer, Amount = amount, Destination = destination, Timestamp = DateTime.UtcNow };
        }

        public static Transaction DepositBitcoin(float amountInBitcoin, float amountInEthereum)
        {
            return new Transaction { Type = TransactionType.DepositBitcoin, Amount = (int)amountInEthereum, AmountInBitcoin = amountInBitcoin, AmountInEthereum = amountInEthereum, Timestamp = DateTime.UtcNow };
        }

        public static Transaction WithdrawBitcoin(float amountInBitcoin, float amountInEthereum)
        {
            return new Transaction { Type = TransactionType.WithdrawBitcoin, Amount = (int)amountInEthereum, AmountInBitcoin = amountInBitcoin, AmountInEthereum = amountInEthereum, Timestamp = DateTime.UtcNow };
        }

        public static Transaction TransferBitcoin(float amountInBitcoin, float amountInEthereum, Wallet destination)
        {
            return new Transaction { Type = TransactionType.TransferBitcoin, Amount = (int)amountInEthereum, AmountInBitcoin = amountInBitcoin, AmountInEthereum = amountInEthereum, Destination = destination, Timestamp = DateTime.UtcNow };
        }
    }
}
