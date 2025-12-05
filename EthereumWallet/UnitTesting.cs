using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using NUnit.Framework;

namespace EthereumWallet
{
    [TestFixture]
    public class UnitTesting
    {
        [Test]
        [Category("pass")]
        [TestCase(0.00F)]
        [TestCase(350000.00F)]
        public void CreateAccountOK(float a)
        {
            //arrange + act
            var source = new Wallet(a);

            //assert
            Assert.AreEqual(a, source.Balance);
            Assert.AreEqual(1, source.Transactions.Count);
            Assert.AreEqual(Transaction.TransactionType.Initial, source.Transactions[0].Type);
        }

        [Test]
        [Category("fail")]
        [TestCase(-0.01F)]
        [TestCase(-100.00F)]
        public void CreateAccountNegative(float a)
        {
            Assert.That(() => new Wallet(a), Throws.TypeOf<NegativeCreateException>());
        }

        [Test]
        [Category("pass")]
        [TestCase(0)]
        [TestCase(50000)]
        [TestCase(4980)]
        public void DepositOK(int a)
        {
            //arrange
            var source = new Wallet(100);

            //act
            source.Deposit(a);

            //assert
            Assert.AreEqual(a + 100, source.Balance);
            Assert.AreEqual(Transaction.TransactionType.Deposit, source.Transactions[source.Transactions.Count - 1].Type);
        }

        [Test]
        [Category("fail")]
        [TestCase(-10)]
        [TestCase(-150)]
        public void DepositNegative(int a)
        {
            //arrange
            var source = new Wallet(100);

            //act
            Assert.That(() => source.Deposit(a), Throws.TypeOf<NegativeDepositException>());
        }

        [Test]
        [Category("fail")]
        [TestCase(1)]
        [TestCase(3999)]
        public void DepositDivision(int a)
        {
            //arrange
            var source = new Wallet(100);

            //act
            Assert.That(() => source.Deposit(a), Throws.TypeOf<DivisionDepositException>());
        }

        [Test]
        [Category("fail")]
        [TestCase(100200)]
        public void DepositTooMuch(int a)
        {
            //arrange
            var source = new Wallet(100);

            //act
            Assert.That(() => source.Deposit(a), Throws.TypeOf<TooMuchDepositException>());
        }

        [Test]
        [Category("pass")]
        [TestCase(100, 50)]
        [TestCase(1000, 10)]
        public void WithdrawOK(int start, int amount)
        {
            //arrange
            var source = new Wallet(start);

            //act
            source.Withdraw(amount);

            //assert
            Assert.AreEqual(start - amount, source.Balance);
            Assert.AreEqual(Transaction.TransactionType.Withdraw, source.Transactions[source.Transactions.Count - 1].Type);
        }

        [Test]
        [Category("fail")]
        [TestCase(100, 110)]
        public void WithdrawNotEnough(int start, int amount)
        {
            //arrange
            var source = new Wallet(start);

            //act
            Assert.That(() => source.Withdraw(amount), Throws.TypeOf<NotEnoughFundsWithdrawException>());
        }

        [Test]
        [Category("fail")]
        [TestCase(100, -10)]
        public void WithdrawNegative(int start, int amount)
        {
            //arrange
            var source = new Wallet(start);

            //act
            Assert.That(() => source.Withdraw(amount), Throws.TypeOf<NegativeWithdrawException>());
        }

        [Test]
        [Category("fail")]
        [TestCase(100, 11)]
        public void WithdrawDivision(int start, int amount)
        {
            //arrange
            var source = new Wallet(start);

            //act
            Assert.That(() => source.Withdraw(amount), Throws.TypeOf<DivisionWithdrawException>());
        }

        [Test]
        [Category("pass")]
        [TestCase(200.00F, 150.00F, 0)]
        [TestCase(200.00F, 400.00F, 200)]
        [TestCase(200.00F, 140.00F, 140)]
        public void TransferEthOK(float a, float b, int c)
        {
            //arrange
            var source = new Wallet(a);
            var destination = new Wallet(b);

            //act
            source.Transfer(destination, c);

            //assert
            Assert.AreEqual(b + c, destination.Balance);
            Assert.AreEqual(a - c, source.Balance);
            Assert.AreEqual(Transaction.TransactionType.Transfer, source.Transactions[source.Transactions.Count - 1].Type);
        }

        [Test]
        [Category("fail")]
        [TestCase(200.00F, 650.00F, 10)]
        [TestCase(200.00F, 400.00F, 800)]
        public void MockTransferFunds(float a, float b, int c)
        {
            //arrange
            var source = new Wallet(a);
            var destination = new Wallet(b);
            var convertorMock = new Mock<ICurrencyConverterStub>();

            //arrange TDbl
            convertorMock.Setup(_ => _.BitcoinToEthereum(20)).Returns(20 * c);

            //act
            try
            {
                source.TransferBitcoin(destination, 20.00f, convertorMock.Object);
            }
            catch (Exception)
            {
                // Ignore domain exceptions; the purpose here is to verify converter interaction.
            }

            //assert -
            convertorMock.Verify(_ => _.BitcoinToEthereum(20), Times.AtLeastOnce());
        }

        [Test]
        [Category("pass")]
        [TestCase(0.00F, 4.00F)]
        [TestCase(12500.00F, 4.00F)]
        [TestCase(1245.00F, 4.00F)]
        public void StubDepositBitcoinOK(float a, float b)
        {
            //arrange 
            ICurrencyConverterStub converter = new CurrencyConverterStub(b);
            var source = new Wallet(100);

            //act 
            source.DepositBitcoin(a, converter);

            //assert
            Assert.AreEqual(100 + converter.BitcoinToEthereum(a), source.Balance);
            Assert.AreEqual(Transaction.TransactionType.DepositBitcoin, source.Transactions[source.Transactions.Count - 1].Type);
        }

        [Test]
        [Category("fail")]
        [TestCase(-2.50F, 4.00F)]
        [TestCase(-37.50F, 4.00F)]
        public void DepositBitcoinNegative(float a, float b)
        {
            //arrange 
            ICurrencyConverterStub converter = new CurrencyConverterStub(b);
            var source = new Wallet(100);

            //act
            Assert.That(() => source.DepositBitcoin(a, converter), Throws.TypeOf<NegativeDepositException>());
        }

        [Test]
        [Category("pass")]
        [TestCase(25.00F, 4.00F)]
        [TestCase(0.00F, 4.00F)]
        [TestCase(10.00F, 4.00F)]
        public void WithdrawBitcoinOK(float a, float b)
        {
            //arrange 
            ICurrencyConverterStub converter = new CurrencyConverterStub(b);
            var source = new Wallet(100);

            //act
            source.WithdrawBitcoin(a, converter);

            //assert
            Assert.AreEqual(100 - converter.BitcoinToEthereum(a), source.Balance);
            Assert.AreEqual(Transaction.TransactionType.WithdrawBitcoin, source.Transactions[source.Transactions.Count - 1].Type);
        }

        [Test]
        [Category("pass")]
        [TestCase(200.00F, 150.00F, 0.00F, 4.00F)]
        [TestCase(200.00F, 400.00F, 50.00F, 4.00F)]
        [TestCase(200.00F, 140.00F, 35.00F, 4.00F)]
        public void TransferBtcOK(float a, float b, float c, float d)
        {
            //arrange 
            ICurrencyConverterStub converter = new CurrencyConverterStub(d);
            var source = new Wallet(a);
            var destination = new Wallet(b);

            //act 
            source.TransferBitcoin(destination, c, converter);

            //assert
            Assert.AreEqual(b + converter.BitcoinToEthereum(c), destination.Balance);
            Assert.AreEqual(a - converter.BitcoinToEthereum(c), source.Balance);
            Assert.AreEqual(Transaction.TransactionType.TransferBitcoin, source.Transactions[source.Transactions.Count - 1].Type);
        }

        [Test]
        [Category("fail")]
        [TestCase(500.00F, 650.00F, 127.50F, 4.00F)]
        [TestCase(200.00F, 400.00F, 200.00F, 4.00F)]
        public void TransferBitcoinNotEnoughFunds(float a, float b, float c, float d)
        {
            //arrange 
            ICurrencyConverterStub converter = new CurrencyConverterStub(d);
            var source = new Wallet(a);
            var destination = new Wallet(b);

            //act 
            Assert.That(() => source.TransferBitcoin(destination, c, converter), Throws.TypeOf<NotEnoughFundsTransferException>());
        }

        [Test]
        [Category("fail")]
        [TestCase(200.00F, 650.00F, -2.50F, 4.00F)]
        [TestCase(200.00F, 400.00F, -200.00F, 4.00F)]
        public void TransferBitcoinNegative(float a, float b, float c, float d)
        {
            //arrange 
            ICurrencyConverterStub converter = new CurrencyConverterStub(d);
            var source = new Wallet(a);
            var destination = new Wallet(b);

            //act 
            Assert.That(() => source.TransferBitcoin(destination, c, converter), Throws.TypeOf<NegativeTransferException>());
        }

        [Test]
        [Category("fail")]
        [TestCase(200.00F, 650.00F, 0.25F, 4.00F)]
        [TestCase(200.00F, 400.00F, 13.25F, 4.00F)]
        [TestCase(200.00F, 400.00F, 49.75F, 4.00F)]
        public void TransferBitcoinDivision(float a, float b, float c, float d)
        {
            //arrange 
            ICurrencyConverterStub converter = new CurrencyConverterStub(d);
            var source = new Wallet(a);
            var destination = new Wallet(b);

            //act 
            Assert.That(() => source.TransferBitcoin(destination, c, converter), Throws.TypeOf<DivisionTransferException>());
        }

        [Test]
        [Category("fail")]
        [TestCase(200000.00F, 650.00F, 12502.50F, 4.00F)]
        [TestCase(200000.00F, 400.00F, 25057.50F, 4.00F)]
        public void TransferBitcoinTooMuch(float a, float b, float c, float d)
        {
            //arrange 
            ICurrencyConverterStub converter = new CurrencyConverterStub(d);
            var source = new Wallet(a);
            var destination = new Wallet(b);

            //act 
            Assert.That(() => source.TransferBitcoin(destination, c, converter), Throws.TypeOf<TooMuchTransferException>());
        }
    }
}
