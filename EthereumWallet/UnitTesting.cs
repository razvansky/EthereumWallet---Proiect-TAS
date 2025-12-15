using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace EthereumWallet
{
    /// <summary>
    /// =============================================================================
    /// COMPLETE TEST SUITE - Domain Testing, TDD, Test Doubles, Full Coverage
    /// =============================================================================
    /// </summary>
    [TestFixture]
    public class UnitTesting
    {
        #region ==================== WALLET CREATION TESTS ====================

        /// <summary>
        /// Domain Testing: Clase de echivalenta pentru creare wallet
        /// - Clasa valida: amount >= 0
        /// - Clasa invalida: amount < 0
        /// </summary>
        [Test]
        [Category("pass")]
        [Category("DomainTesting")]
        [TestCase(0.00F, Description = "Boundary: minimum valid (ON)")]
        [TestCase(0.01F, Description = "Boundary: just above minimum (OFF)")]
        [TestCase(100.00F, Description = "Equivalence: typical value")]
        [TestCase(350000.00F, Description = "Equivalence: large valid value")]
        public void CreateAccountOK(float a)
        {
            // Arrange + Act
            var source = new Wallet(a);

            // Assert
            Assert.That(source.Balance, Is.EqualTo(a));
            Assert.That(source.Transactions.Count, Is.EqualTo(1));
            Assert.That(source.Transactions[0].Type, Is.EqualTo(Transaction.TransactionType.Initial));
        }

        [Test]
        [Category("fail")]
        [Category("DomainTesting")]
        [TestCase(-0.01F, Description = "Boundary: just below minimum (OFF)")]
        [TestCase(-1.00F, Description = "Equivalence: negative value")]
        [TestCase(-100.00F, Description = "Equivalence: large negative")]
        public void CreateAccountNegative(float a)
        {
            Assert.That(() => new Wallet(a), Throws.TypeOf<NegativeCreateException>());
        }

        #endregion

        #region ==================== DEPOSIT DOMAIN TESTING ====================

        /// <summary>
        /// Domain Testing pentru Deposit:
        /// 
        /// Domenii:
        /// D1: amount < 0           -> NegativeDepositException
        /// D2: amount % 10 != 0     -> DivisionDepositException  
        /// D3: amount > 50000       -> TooMuchDepositException
        /// D4: 0 <= amount <= 50000 && amount % 10 == 0 -> SUCCESS
        /// 
        /// Boundary Values: -1, 0, 1, 9, 10, 11, 49990, 50000, 50001, 50010
        /// </summary>
        [Test]
        [Category("pass")]
        [Category("DomainTesting")]
        [Category("BoundaryValueAnalysis")]
        [TestCase(0, Description = "Boundary: minimum valid (ON point)")]
        [TestCase(10, Description = "Boundary: first valid non-zero (ON point)")]
        [TestCase(50000, Description = "Boundary: maximum valid (ON point)")]
        [TestCase(4980, Description = "Equivalence: typical valid value")]
        [TestCase(49990, Description = "Boundary: near maximum (IN point)")]
        public void Deposit_BoundaryValid_Success(int amount)
        {
            // Arrange
            var wallet = new Wallet(100);
            var expectedBalance = 100 + amount;

            // Act
            wallet.Deposit(amount);

            // Assert
            Assert.That(wallet.Balance, Is.EqualTo(expectedBalance));
            Assert.That(wallet.Transactions[^1].Type, Is.EqualTo(Transaction.TransactionType.Deposit));
        }

        [Test]
        [Category("fail")]
        [Category("DomainTesting")]
        [Category("BoundaryValueAnalysis")]
        [TestCase(-1, Description = "Boundary: just below zero (OFF point)")]
        [TestCase(-10, Description = "Equivalence: negative multiple of 10")]
        [TestCase(-150, Description = "Equivalence: negative value")]
        public void Deposit_NegativeAmount_ThrowsException(int amount)
        {
            // Arrange
            var wallet = new Wallet(100);

            // Act & Assert
            Assert.That(() => wallet.Deposit(amount), Throws.TypeOf<NegativeDepositException>());
        }

        [Test]
        [Category("fail")]
        [Category("DomainTesting")]
        [Category("BoundaryValueAnalysis")]
        [TestCase(1, Description = "Boundary: smallest non-multiple of 10 (OFF point)")]
        [TestCase(9, Description = "Boundary: just before first valid (OFF point)")]
        [TestCase(11, Description = "Boundary: just after first valid (OFF point)")]
        [TestCase(3999, Description = "Equivalence: typical non-multiple")]
        [TestCase(49999, Description = "Boundary: near max, not multiple")]
        public void Deposit_NotMultipleOf10_ThrowsException(int amount)
        {
            // Arrange
            var wallet = new Wallet(100);

            // Act & Assert
            Assert.That(() => wallet.Deposit(amount), Throws.TypeOf<DivisionDepositException>());
        }

        [Test]
        [Category("fail")]
        [Category("DomainTesting")]
        [Category("BoundaryValueAnalysis")]
        [TestCase(50010, Description = "Boundary: first invalid above max (OFF point)")]
        [TestCase(50020, Description = "Boundary: just above maximum, multiple of 10")]
        [TestCase(100200, Description = "Equivalence: large invalid value")]
        public void Deposit_ExceedsLimit_ThrowsException(int amount)
        {
            // Arrange
            var wallet = new Wallet(100);

            // Act & Assert
            Assert.That(() => wallet.Deposit(amount), Throws.TypeOf<TooMuchDepositException>());
        }

        #endregion

        #region ==================== WITHDRAW DOMAIN TESTING ====================

        /// <summary>
        /// Domain Testing pentru Withdraw:
        /// 
        /// Domenii (in ordinea verificarii):
        /// D1: amount > balance     -> NotEnoughFundsWithdrawException
        /// D2: amount < 0           -> NegativeWithdrawException
        /// D3: amount % 10 != 0     -> DivisionWithdrawException
        /// D4: amount > 50000       -> TooMuchWithdrawException
        /// D5: Valid                -> SUCCESS
        /// 
        /// Boundary Values cu balance=100: 0, 10, 90, 100, 110
        /// </summary>
        [Test]
        [Category("pass")]
        [Category("DomainTesting")]
        [TestCase(100, 0, Description = "Boundary: withdraw zero")]
        [TestCase(100, 10, Description = "Boundary: minimum non-zero withdraw")]
        [TestCase(100, 50, Description = "Equivalence: typical withdraw")]
        [TestCase(100, 100, Description = "Boundary: withdraw exact balance (ON point)")]
        [TestCase(1000, 1000, Description = "Boundary: withdraw exact large balance")]
        public void Withdraw_ValidAmount_Success(int startBalance, int withdrawAmount)
        {
            // Arrange
            var wallet = new Wallet(startBalance);
            var expectedBalance = startBalance - withdrawAmount;

            // Act
            wallet.Withdraw(withdrawAmount);

            // Assert
            Assert.That(wallet.Balance, Is.EqualTo(expectedBalance));
            Assert.That(wallet.Transactions[^1].Type, Is.EqualTo(Transaction.TransactionType.Withdraw));
        }

        [Test]
        [Category("fail")]
        [Category("DomainTesting")]
        [TestCase(100, 110, Description = "Boundary: just above balance (OFF point)")]
        [TestCase(100, 200, Description = "Equivalence: much more than balance")]
        [TestCase(50, 100, Description = "Equivalence: double the balance")]
        public void Withdraw_InsufficientFunds_ThrowsException(int startBalance, int withdrawAmount)
        {
            // Arrange
            var wallet = new Wallet(startBalance);

            // Act & Assert
            Assert.That(() => wallet.Withdraw(withdrawAmount), Throws.TypeOf<NotEnoughFundsWithdrawException>());
        }

        [Test]
        [Category("fail")]
        [Category("DomainTesting")]
        [TestCase(100, -10, Description = "Equivalence: negative withdraw")]
        [TestCase(100, -1, Description = "Boundary: just below zero")]
        public void Withdraw_NegativeAmount_ThrowsException(int startBalance, int withdrawAmount)
        {
            // Arrange
            var wallet = new Wallet(startBalance);

            // Act & Assert
            Assert.That(() => wallet.Withdraw(withdrawAmount), Throws.TypeOf<NegativeWithdrawException>());
        }

        [Test]
        [Category("fail")]
        [Category("DomainTesting")]
        [TestCase(100, 11, Description = "Boundary: not multiple of 10")]
        [TestCase(100, 99, Description = "Equivalence: valid amount but not multiple")]
        public void Withdraw_NotMultipleOf10_ThrowsException(int startBalance, int withdrawAmount)
        {
            // Arrange
            var wallet = new Wallet(startBalance);

            // Act & Assert
            Assert.That(() => wallet.Withdraw(withdrawAmount), Throws.TypeOf<DivisionWithdrawException>());
        }

        [Test]
        [Category("fail")]
        [Category("DomainTesting")]
        [TestCase(100000, 50010, Description = "Boundary: just above limit")]
        [TestCase(100000, 60000, Description = "Equivalence: exceeds limit")]
        public void Withdraw_ExceedsLimit_ThrowsException(int startBalance, int withdrawAmount)
        {
            // Arrange
            var wallet = new Wallet(startBalance);

            // Act & Assert
            Assert.That(() => wallet.Withdraw(withdrawAmount), Throws.TypeOf<TooMuchWithdrawException>());
        }

        #endregion

        #region ==================== TRANSFER DOMAIN TESTING ====================

        /// <summary>
        /// Domain Testing pentru Transfer ETH
        /// </summary>
        [Test]
        [Category("pass")]
        [Category("DomainTesting")]
        [TestCase(200.00F, 150.00F, 0, Description = "Boundary: transfer zero")]
        [TestCase(200.00F, 400.00F, 200, Description = "Boundary: transfer exact balance")]
        [TestCase(200.00F, 140.00F, 140, Description = "Equivalence: typical transfer")]
        [TestCase(1000.00F, 500.00F, 500, Description = "Equivalence: half balance")]
        public void Transfer_ValidAmount_Success(float sourceBalance, float destBalance, int amount)
        {
            // Arrange
            var source = new Wallet(sourceBalance);
            var destination = new Wallet(destBalance);

            // Act
            source.Transfer(destination, amount);

            // Assert
            Assert.That(destination.Balance, Is.EqualTo(destBalance + amount));
            Assert.That(source.Balance, Is.EqualTo(sourceBalance - amount));
            Assert.That(source.Transactions[^1].Type, Is.EqualTo(Transaction.TransactionType.Transfer));
        }

        [Test]
        [Category("fail")]
        [Category("DomainTesting")]
        [TestCase(100.00F, 200.00F, 110, Description = "Boundary: just above source balance")]
        [TestCase(50.00F, 500.00F, 100, Description = "Equivalence: double source balance")]
        public void Transfer_InsufficientFunds_ThrowsException(float sourceBalance, float destBalance, int amount)
        {
            // Arrange
            var source = new Wallet(sourceBalance);
            var destination = new Wallet(destBalance);

            // Act & Assert
            Assert.That(() => source.Transfer(destination, amount), Throws.TypeOf<NotEnoughFundsTransferException>());
        }

        [Test]
        [Category("fail")]
        [Category("DomainTesting")]
        [TestCase(200.00F, 100.00F, -10)]
        [TestCase(200.00F, 100.00F, -1)]
        public void Transfer_NegativeAmount_ThrowsException(float sourceBalance, float destBalance, int amount)
        {
            var source = new Wallet(sourceBalance);
            var destination = new Wallet(destBalance);

            Assert.That(() => source.Transfer(destination, amount), Throws.TypeOf<NegativeTransferException>());
        }

        [Test]
        [Category("fail")]
        [Category("DomainTesting")]
        [TestCase(200.00F, 100.00F, 11)]
        [TestCase(200.00F, 100.00F, 99)]
        public void Transfer_NotMultipleOf10_ThrowsException(float sourceBalance, float destBalance, int amount)
        {
            var source = new Wallet(sourceBalance);
            var destination = new Wallet(destBalance);

            Assert.That(() => source.Transfer(destination, amount), Throws.TypeOf<DivisionTransferException>());
        }

        [Test]
        [Category("fail")]
        [Category("DomainTesting")]
        [TestCase(100000.00F, 100.00F, 50010)]
        public void Transfer_ExceedsLimit_ThrowsException(float sourceBalance, float destBalance, int amount)
        {
            var source = new Wallet(sourceBalance);
            var destination = new Wallet(destBalance);

            Assert.That(() => source.Transfer(destination, amount), Throws.TypeOf<TooMuchTransferException>());
        }

        [Test]
        [Category("fail")]
        public void Transfer_NullDestination_ThrowsArgumentNullException()
        {
            var source = new Wallet(100);

            Assert.That(() => source.Transfer(null!, 10), Throws.TypeOf<ArgumentNullException>());
        }

        #endregion

        #region ==================== TEST DOUBLES: STUB ====================

        /// <summary>
        /// STUB: Folosim CurrencyConverterStub - implementare simplificata cu rate fixe
        /// </summary>
        [Test]
        [Category("pass")]
        [Category("TestDouble")]
        [Category("Stub")]
        [TestCase(0.00F, 4.00F, Description = "Stub: zero bitcoin deposit")]
        [TestCase(12500.00F, 4.00F, Description = "Stub: large bitcoin deposit")]
        [TestCase(1245.00F, 4.00F, Description = "Stub: typical bitcoin deposit")]
        public void Stub_DepositBitcoin_ConvertsCorrectly(float btcAmount, float rate)
        {
            // Arrange - STUB cu rate fixa
            ICurrencyConverterStub stubConverter = new CurrencyConverterStub(rate);
            var wallet = new Wallet(100);
            var expectedEth = btcAmount * rate;

            // Act
            wallet.DepositBitcoin(btcAmount, stubConverter);

            // Assert
            Assert.That(wallet.Balance, Is.EqualTo(100 + (int)expectedEth));
            Assert.That(wallet.Transactions[^1].Type, Is.EqualTo(Transaction.TransactionType.DepositBitcoin));
        }

        [Test]
        [Category("fail")]
        [Category("TestDouble")]
        [Category("Stub")]
        [TestCase(-2.50F, 4.00F)]
        [TestCase(-37.50F, 4.00F)]
        public void Stub_DepositBitcoinNegative_ThrowsException(float btcAmount, float rate)
        {
            ICurrencyConverterStub stubConverter = new CurrencyConverterStub(rate);
            var wallet = new Wallet(100);

            Assert.That(() => wallet.DepositBitcoin(btcAmount, stubConverter), Throws.TypeOf<NegativeDepositException>());
        }

        [Test]
        [Category("pass")]
        [Category("TestDouble")]
        [Category("Stub")]
        [TestCase(25.00F, 4.00F)]
        [TestCase(0.00F, 4.00F)]
        [TestCase(10.00F, 4.00F)]
        public void Stub_WithdrawBitcoin_ConvertsCorrectly(float btcAmount, float rate)
        {
            ICurrencyConverterStub stubConverter = new CurrencyConverterStub(rate);
            var wallet = new Wallet(100);
            var expectedEth = btcAmount * rate;

            wallet.WithdrawBitcoin(btcAmount, stubConverter);

            Assert.That(wallet.Balance, Is.EqualTo(100 - (int)expectedEth));
            Assert.That(wallet.Transactions[^1].Type, Is.EqualTo(Transaction.TransactionType.WithdrawBitcoin));
        }

        [Test]
        [Category("pass")]
        [Category("TestDouble")]
        [Category("Stub")]
        [TestCase(200.00F, 150.00F, 0.00F, 4.00F)]
        [TestCase(200.00F, 400.00F, 50.00F, 4.00F)]
        [TestCase(200.00F, 140.00F, 35.00F, 4.00F)]
        public void Stub_TransferBitcoin_ConvertsCorrectly(float srcBalance, float destBalance, float btcAmount, float rate)
        {
            ICurrencyConverterStub stubConverter = new CurrencyConverterStub(rate);
            var source = new Wallet(srcBalance);
            var destination = new Wallet(destBalance);
            var ethAmount = btcAmount * rate;

            source.TransferBitcoin(destination, btcAmount, stubConverter);

            Assert.That(destination.Balance, Is.EqualTo(destBalance + (int)ethAmount));
            Assert.That(source.Balance, Is.EqualTo(srcBalance - (int)ethAmount));
        }

        #endregion

        #region ==================== TEST DOUBLES: MOCK ====================

        /// <summary>
        /// MOCK: Folosim Moq pentru a verifica interactiunile cu dependentele
        /// Mock-ul nu are implementare reala, doar inregistreaza apelurile
        /// </summary>
        [Test]
        [Category("TestDouble")]
        [Category("Mock")]
        public void Mock_TransferBitcoin_VerifiesConverterIsCalled()
        {
            // Arrange
            var source = new Wallet(500);
            var destination = new Wallet(100);
            var mockConverter = new Mock<ICurrencyConverterStub>();
            
            // Setup mock sa returneze o valoare valida (multiplu de 10, <= 50000)
            mockConverter.Setup(c => c.BitcoinToEthereum(25.00f)).Returns(100f);

            // Act
            source.TransferBitcoin(destination, 25.00f, mockConverter.Object);

            // Assert - Verificam ca metoda a fost apelata exact o data
            mockConverter.Verify(c => c.BitcoinToEthereum(25.00f), Times.Once());
        }

        [Test]
        [Category("TestDouble")]
        [Category("Mock")]
        public void Mock_DepositBitcoin_VerifiesConverterInteraction()
        {
            // Arrange
            var wallet = new Wallet(100);
            var mockConverter = new Mock<ICurrencyConverterStub>();
            mockConverter.Setup(c => c.BitcoinToEthereum(It.IsAny<float>())).Returns(50f);

            // Act
            wallet.DepositBitcoin(12.5f, mockConverter.Object);

            // Assert
            mockConverter.Verify(c => c.BitcoinToEthereum(12.5f), Times.AtLeastOnce());
        }

        [Test]
        [Category("TestDouble")]
        [Category("Mock")]
        public void Mock_WithdrawBitcoin_VerifiesConverterCalled()
        {
            // Arrange
            var wallet = new Wallet(200);
            var mockConverter = new Mock<ICurrencyConverterStub>();
            mockConverter.Setup(c => c.BitcoinToEthereum(10.0f)).Returns(40f);

            // Act
            wallet.WithdrawBitcoin(10.0f, mockConverter.Object);

            // Assert
            mockConverter.Verify(c => c.BitcoinToEthereum(10.0f), Times.Exactly(1));
        }

        [Test]
        [Category("TestDouble")]
        [Category("Mock")]
        public void Mock_MultipleOperations_VerifiesAllCalls()
        {
            // Arrange
            var wallet = new Wallet(1000);
            var mockConverter = new Mock<ICurrencyConverterStub>();
            mockConverter.Setup(c => c.BitcoinToEthereum(It.IsAny<float>())).Returns(100f);

            // Act - Multiple operations
            wallet.DepositBitcoin(5.0f, mockConverter.Object);
            wallet.DepositBitcoin(10.0f, mockConverter.Object);
            wallet.WithdrawBitcoin(2.5f, mockConverter.Object);

            // Assert - Verificam numarul total de apeluri
            mockConverter.Verify(c => c.BitcoinToEthereum(It.IsAny<float>()), Times.Exactly(3));
        }

        [Test]
        [Category("fail")]
        [Category("TestDouble")]
        [Category("Mock")]
        [TestCase(200.00F, 650.00F, 10)]
        [TestCase(200.00F, 400.00F, 800)]
        public void Mock_TransferWithException_StillVerifiesCall(float a, float b, int c)
        {
            // Arrange
            var source = new Wallet(a);
            var destination = new Wallet(b);
            var mockConverter = new Mock<ICurrencyConverterStub>();
            mockConverter.Setup(m => m.BitcoinToEthereum(20)).Returns(20 * c);

            // Act - Poate arunca exceptie, dar vrem sa verificam ca s-a apelat
            try
            {
                source.TransferBitcoin(destination, 20.00f, mockConverter.Object);
            }
            catch (Exception)
            {
                // Ignore - verificam doar apelul
            }

            // Assert
            mockConverter.Verify(m => m.BitcoinToEthereum(20), Times.AtLeastOnce());
        }

        #endregion

        #region ==================== TEST DOUBLES: FAKE ====================

        /// <summary>
        /// FAKE: Implementare completa dar simplificata
        /// Diferenta fata de Stub: Fake-ul are logica reala, nu doar valori hardcodate
        /// </summary>
        [Test]
        [Category("pass")]
        [Category("TestDouble")]
        [Category("Fake")]
        public void Fake_DepositBitcoin_UsesInternalRates()
        {
            // Arrange - Fake cu rate interne realiste
            var fakeConverter = new FakeCurrencyConverter(10.0f); // 1 BTC = 10 ETH
            var wallet = new Wallet(100);

            // Act
            wallet.DepositBitcoin(5.0f, fakeConverter);

            // Assert - 5 BTC * 10 = 50 ETH
            Assert.That(wallet.Balance, Is.EqualTo(150));
        }

        [Test]
        [Category("pass")]
        [Category("TestDouble")]
        [Category("Fake")]
        public void Fake_TracksConversionHistory()
        {
            // Arrange
            var fakeConverter = new FakeCurrencyConverter(4.0f);
            var wallet = new Wallet(1000);

            // Act
            wallet.DepositBitcoin(10.0f, fakeConverter);
            wallet.WithdrawBitcoin(5.0f, fakeConverter);

            // Assert - Verificam istoricul conversiilor
            var history = fakeConverter.GetConversionHistory();
            Assert.That(history.Count, Is.GreaterThanOrEqualTo(2));
        }

        [Test]
        [Category("pass")]
        [Category("TestDouble")]
        [Category("Fake")]
        public void Fake_AllowsRateModification()
        {
            // Arrange
            var fakeConverter = new FakeCurrencyConverter();
            fakeConverter.SetExchangeRate("BTC_ETH", 20.0f);
            var wallet = new Wallet(100);

            // Act
            wallet.DepositBitcoin(1.0f, fakeConverter);

            // Assert - 1 BTC * 20 = 20 ETH
            Assert.That(wallet.Balance, Is.EqualTo(120));
        }

        [Test]
        [Category("pass")]
        [Category("TestDouble")]
        [Category("Fake")]
        public void FakeRepository_SaveAndRetrieve()
        {
            // Arrange
            var fakeRepo = new FakeWalletRepository();
            var wallet = new Wallet(500);

            // Act
            fakeRepo.Save(wallet);
            var id = fakeRepo.GetWalletId(wallet);
            var retrieved = fakeRepo.GetById(id);

            // Assert
            Assert.That(retrieved, Is.SameAs(wallet));
            Assert.That(fakeRepo.Count, Is.EqualTo(1));
        }

        [Test]
        [Category("pass")]
        [Category("TestDouble")]
        [Category("Fake")]
        public void FakeRepository_DeleteWallet()
        {
            // Arrange
            var fakeRepo = new FakeWalletRepository();
            var wallet = new Wallet(100);
            fakeRepo.Save(wallet);
            var id = fakeRepo.GetWalletId(wallet);

            // Act
            fakeRepo.Delete(id);

            // Assert
            Assert.That(fakeRepo.Exists(id), Is.False);
            Assert.That(fakeRepo.Count, Is.EqualTo(0));
        }

        [Test]
        [Category("pass")]
        [Category("TestDouble")]
        [Category("Fake")]
        public void FakeRepository_GetAll()
        {
            // Arrange
            var fakeRepo = new FakeWalletRepository();
            var wallet1 = new Wallet(100);
            var wallet2 = new Wallet(200);
            var wallet3 = new Wallet(300);

            // Act
            fakeRepo.Save(wallet1);
            fakeRepo.Save(wallet2);
            fakeRepo.Save(wallet3);

            // Assert
            var allWallets = fakeRepo.GetAll().ToList();
            Assert.That(allWallets.Count, Is.EqualTo(3));
            Assert.That(allWallets, Contains.Item(wallet1));
            Assert.That(allWallets, Contains.Item(wallet2));
            Assert.That(allWallets, Contains.Item(wallet3));
        }

        #endregion

        #region ==================== TEST DOUBLES: SPY ====================

        /// <summary>
        /// SPY: Wrapper care inregistreaza toate apelurile
        /// Diferenta fata de Mock: Spy-ul foloseste implementarea REALA si doar inregistreaza
        /// </summary>
        [Test]
        [Category("pass")]
        [Category("TestDouble")]
        [Category("Spy")]
        public void Spy_TracksMethodCalls()
        {
            // Arrange
            var realConverter = new CurrencyConverterStub(5.0f);
            var spyConverter = new SpyCurrencyConverter(realConverter);
            var wallet = new Wallet(1000);

            // Act
            wallet.DepositBitcoin(10.0f, spyConverter);
            wallet.DepositBitcoin(20.0f, spyConverter);

            // Assert - Spy-ul a inregistrat apelurile
            Assert.That(spyConverter.TotalCalls, Is.GreaterThanOrEqualTo(2));
            Assert.That(spyConverter.BitcoinToEthereumCallCount, Is.GreaterThanOrEqualTo(2));
        }

        [Test]
        [Category("pass")]
        [Category("TestDouble")]
        [Category("Spy")]
        public void Spy_VerifiesSpecificCallWasMade()
        {
            // Arrange
            var realConverter = new CurrencyConverterStub(4.0f);
            var spyConverter = new SpyCurrencyConverter(realConverter);
            var wallet = new Wallet(500);

            // Act
            wallet.DepositBitcoin(25.0f, spyConverter);

            // Assert - Verificam ca a fost apelat cu valoarea specifica
            Assert.That(spyConverter.WasCalledWith("BitcoinToEthereum", 25.0f), Is.True);
        }

        [Test]
        [Category("pass")]
        [Category("TestDouble")]
        [Category("Spy")]
        public void Spy_RecordsInputAndOutput()
        {
            // Arrange
            var realConverter = new CurrencyConverterStub(2.0f);
            var spyConverter = new SpyCurrencyConverter(realConverter);
            var wallet = new Wallet(200);

            // Act
            wallet.DepositBitcoin(50.0f, spyConverter);

            // Assert - Verificam ca inputul si outputul sunt inregistrate
            var calls = spyConverter.GetAllCalls();
            Assert.That(calls.Count, Is.GreaterThan(0));
            var lastCall = calls[^1];
            Assert.That(lastCall.Input, Is.EqualTo(50.0f));
            Assert.That(lastCall.Output, Is.EqualTo(100.0f)); // 50 * 2 = 100
        }

        [Test]
        [Category("pass")]
        [Category("TestDouble")]
        [Category("Spy")]
        public void Spy_UsesRealImplementation()
        {
            // Arrange - Spy foloseste convertor REAL
            var realConverter = new CurrencyConverterStub(10.0f);
            var spyConverter = new SpyCurrencyConverter(realConverter);
            var wallet = new Wallet(100);

            // Act
            wallet.DepositBitcoin(5.0f, spyConverter);

            // Assert - Rezultatul este cel real (nu simulat)
            Assert.That(wallet.Balance, Is.EqualTo(150)); // 100 + (5 * 10)
        }

        #endregion

        #region ==================== TEST DOUBLES: DUMMY ====================

        /// <summary>
        /// DUMMY: Obiect care nu face nimic, doar ocupa loc
        /// Folosit cand trebuie sa pasam un parametru dar nu ne intereseaza comportamentul
        /// </summary>
        [Test]
        [Category("pass")]
        [Category("TestDouble")]
        [Category("Dummy")]
        public void Dummy_UsedWhenConverterNotImportant()
        {
            // Arrange - Dummy returneaza mereu 0, deci depozitul va fi 0 ETH
            var dummyConverter = new DummyCurrencyConverter();
            var wallet = new Wallet(100);

            // Act - Depunem 1000 BTC, dar dummy-ul converteste la 0
            wallet.DepositBitcoin(1000.0f, dummyConverter);

            // Assert - Balance ramane 100 pentru ca conversia = 0
            Assert.That(wallet.Balance, Is.EqualTo(100));
        }

        [Test]
        [Category("pass")]
        [Category("TestDouble")]
        [Category("Dummy")]
        public void Dummy_ZeroConversionAlwaysValid()
        {
            // Arrange
            var dummyConverter = new DummyCurrencyConverter();
            var wallet = new Wallet(500);

            // Act - Orice operatie cu dummy va rezulta in 0 ETH
            wallet.WithdrawBitcoin(100.0f, dummyConverter);

            // Assert - Nu s-a retras nimic
            Assert.That(wallet.Balance, Is.EqualTo(500));
        }

        #endregion

        #region ==================== TDD STYLE TESTS ====================

        /// <summary>
        /// TDD (Test-Driven Development) Style Tests
        /// RED -> GREEN -> REFACTOR cycle demonstrated
        /// </summary>
        [Test]
        [Category("TDD")]
        [Category("RED_GREEN_REFACTOR")]
        public void TDD_WalletShouldStartWithInitialTransaction()
        {
            // RED: Aceasta ar fi scrisa INAINTE de implementare
            // GREEN: Implementarea in Wallet constructor
            // REFACTOR: Cod curat
            
            // Arrange & Act
            var wallet = new Wallet(100);

            // Assert - Verificam comportamentul specificat
            Assert.That(wallet.Transactions, Is.Not.Empty);
            Assert.That(wallet.Transactions[0].Type, Is.EqualTo(Transaction.TransactionType.Initial));
        }

        [Test]
        [Category("TDD")]
        [Category("RED_GREEN_REFACTOR")]
        public void TDD_DepositShouldAddToBalance()
        {
            // TDD: Scriem testul INTAI, apoi implementam
            var wallet = new Wallet(50);
            
            wallet.Deposit(100);
            
            Assert.That(wallet.Balance, Is.EqualTo(150));
        }

        [Test]
        [Category("TDD")]
        [Category("RED_GREEN_REFACTOR")]
        public void TDD_WithdrawShouldSubtractFromBalance()
        {
            var wallet = new Wallet(200);
            
            wallet.Withdraw(50);
            
            Assert.That(wallet.Balance, Is.EqualTo(150));
        }

        [Test]
        [Category("TDD")]
        [Category("RED_GREEN_REFACTOR")]
        public void TDD_TransferShouldMoveMoneyBetweenWallets()
        {
            var source = new Wallet(100);
            var destination = new Wallet(50);
            
            source.Transfer(destination, 30);
            
            Assert.Multiple(() =>
            {
                Assert.That(source.Balance, Is.EqualTo(70));
                Assert.That(destination.Balance, Is.EqualTo(80));
            });
        }

        [Test]
        [Category("TDD")]
        public void TDD_TransactionShouldHaveUniqueId()
        {
            var wallet1 = new Wallet(100);
            var wallet2 = new Wallet(200);
            
            Assert.That(wallet1.Transactions[0].Id, Is.Not.EqualTo(wallet2.Transactions[0].Id));
        }

        [Test]
        [Category("TDD")]
        public void TDD_TransactionShouldHaveTimestamp()
        {
            var beforeCreation = DateTime.UtcNow;
            var wallet = new Wallet(100);
            var afterCreation = DateTime.UtcNow;
            
            Assert.That(wallet.Transactions[0].Timestamp, Is.InRange(beforeCreation, afterCreation));
        }

        #endregion

        #region ==================== BITCOIN TRANSFER DOMAIN TESTS ====================

        [Test]
        [Category("fail")]
        [Category("DomainTesting")]
        [TestCase(500.00F, 650.00F, 127.50F, 4.00F)]
        [TestCase(200.00F, 400.00F, 200.00F, 4.00F)]
        public void TransferBitcoin_NotEnoughFunds_ThrowsException(float srcBalance, float destBalance, float btcAmount, float rate)
        {
            ICurrencyConverterStub converter = new CurrencyConverterStub(rate);
            var source = new Wallet(srcBalance);
            var destination = new Wallet(destBalance);

            Assert.That(() => source.TransferBitcoin(destination, btcAmount, converter), 
                Throws.TypeOf<NotEnoughFundsTransferException>());
        }

        [Test]
        [Category("fail")]
        [Category("DomainTesting")]
        [TestCase(200.00F, 650.00F, -2.50F, 4.00F)]
        [TestCase(200.00F, 400.00F, -200.00F, 4.00F)]
        public void TransferBitcoin_NegativeAmount_ThrowsException(float srcBalance, float destBalance, float btcAmount, float rate)
        {
            ICurrencyConverterStub converter = new CurrencyConverterStub(rate);
            var source = new Wallet(srcBalance);
            var destination = new Wallet(destBalance);

            Assert.That(() => source.TransferBitcoin(destination, btcAmount, converter), 
                Throws.TypeOf<NegativeTransferException>());
        }

        [Test]
        [Category("fail")]
        [Category("DomainTesting")]
        [TestCase(200.00F, 650.00F, 0.25F, 4.00F)]
        [TestCase(200.00F, 400.00F, 13.25F, 4.00F)]
        [TestCase(200.00F, 400.00F, 49.75F, 4.00F)]
        public void TransferBitcoin_NotMultipleOf10_ThrowsException(float srcBalance, float destBalance, float btcAmount, float rate)
        {
            ICurrencyConverterStub converter = new CurrencyConverterStub(rate);
            var source = new Wallet(srcBalance);
            var destination = new Wallet(destBalance);

            Assert.That(() => source.TransferBitcoin(destination, btcAmount, converter), 
                Throws.TypeOf<DivisionTransferException>());
        }

        [Test]
        [Category("fail")]
        [Category("DomainTesting")]
        [TestCase(200000.00F, 650.00F, 12502.50F, 4.00F)]
        [TestCase(200000.00F, 400.00F, 25057.50F, 4.00F)]
        public void TransferBitcoin_ExceedsLimit_ThrowsException(float srcBalance, float destBalance, float btcAmount, float rate)
        {
            ICurrencyConverterStub converter = new CurrencyConverterStub(rate);
            var source = new Wallet(srcBalance);
            var destination = new Wallet(destBalance);

            Assert.That(() => source.TransferBitcoin(destination, btcAmount, converter), 
                Throws.TypeOf<TooMuchTransferException>());
        }

        #endregion

        #region ==================== NULL ARGUMENT TESTS (COVERAGE) ====================

        [Test]
        [Category("Coverage")]
        public void DepositBitcoin_NullConverter_ThrowsArgumentNull()
        {
            var wallet = new Wallet(100);
            Assert.That(() => wallet.DepositBitcoin(10f, null!), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        [Category("Coverage")]
        public void WithdrawBitcoin_NullConverter_ThrowsArgumentNull()
        {
            var wallet = new Wallet(100);
            Assert.That(() => wallet.WithdrawBitcoin(10f, null!), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        [Category("Coverage")]
        public void TransferBitcoin_NullDestination_ThrowsArgumentNull()
        {
            var wallet = new Wallet(100);
            var converter = new CurrencyConverterStub(4.0f);
            Assert.That(() => wallet.TransferBitcoin(null!, 10f, converter), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        [Category("Coverage")]
        public void TransferBitcoin_NullConverter_ThrowsArgumentNull()
        {
            var wallet = new Wallet(100);
            var destination = new Wallet(50);
            Assert.That(() => wallet.TransferBitcoin(destination, 10f, null!), Throws.TypeOf<ArgumentNullException>());
        }

        #endregion

        #region ==================== CURRENCY CONVERTER STUB TESTS ====================

        [Test]
        [Category("pass")]
        [Category("Coverage")]
        public void CurrencyConverterStub_BitcoinToEthereum_CalculatesCorrectly()
        {
            var converter = new CurrencyConverterStub(5.0f);
            
            var result = converter.BitcoinToEthereum(10.0f);
            
            Assert.That(result, Is.EqualTo(50.0f));
        }

        [Test]
        [Category("pass")]
        [Category("Coverage")]
        public void CurrencyConverterStub_EthereumToBitcoin_CalculatesCorrectly()
        {
            var converter = new CurrencyConverterStub(5.0f);
            
            var result = converter.EthereumToBitcoin(50.0f);
            
            Assert.That(result, Is.EqualTo(10.0f));
        }

        [Test]
        [Category("fail")]
        [Category("Coverage")]
        public void CurrencyConverterStub_NegativeBitcoin_ThrowsException()
        {
            var converter = new CurrencyConverterStub(5.0f);
            
            Assert.That(() => converter.BitcoinToEthereum(-10.0f), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        [Category("fail")]
        [Category("Coverage")]
        public void CurrencyConverterStub_NegativeEthereum_ThrowsException()
        {
            var converter = new CurrencyConverterStub(5.0f);
            
            Assert.That(() => converter.EthereumToBitcoin(-10.0f), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        [Category("fail")]
        [Category("Coverage")]
        public void CurrencyConverterStub_InvalidRate_ThrowsException()
        {
            Assert.That(() => new CurrencyConverterStub(0), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => new CurrencyConverterStub(-5.0f), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        #endregion

        #region ==================== TRANSACTION FACTORY TESTS ====================

        [Test]
        [Category("Coverage")]
        public void Transaction_AllFactoryMethods_CreateCorrectTypes()
        {
            var wallet = new Wallet(100);
            
            var initial = Transaction.CreateInitial(100);
            var deposit = Transaction.Deposit(50);
            var withdraw = Transaction.Withdraw(30);
            var transfer = Transaction.Transfer(20, wallet);
            var depositBtc = Transaction.DepositBitcoin(5.0f, 25.0f);
            var withdrawBtc = Transaction.WithdrawBitcoin(3.0f, 15.0f);
            var transferBtc = Transaction.TransferBitcoin(2.0f, 10.0f, wallet);
            
            Assert.Multiple(() =>
            {
                Assert.That(initial.Type, Is.EqualTo(Transaction.TransactionType.Initial));
                Assert.That(deposit.Type, Is.EqualTo(Transaction.TransactionType.Deposit));
                Assert.That(withdraw.Type, Is.EqualTo(Transaction.TransactionType.Withdraw));
                Assert.That(transfer.Type, Is.EqualTo(Transaction.TransactionType.Transfer));
                Assert.That(depositBtc.Type, Is.EqualTo(Transaction.TransactionType.DepositBitcoin));
                Assert.That(withdrawBtc.Type, Is.EqualTo(Transaction.TransactionType.WithdrawBitcoin));
                Assert.That(transferBtc.Type, Is.EqualTo(Transaction.TransactionType.TransferBitcoin));
            });
        }

        [Test]
        [Category("Coverage")]
        public void Transaction_BitcoinTransactions_HaveBtcAndEthAmounts()
        {
            var wallet = new Wallet(100);
            
            var depositBtc = Transaction.DepositBitcoin(5.0f, 25.0f);
            var transferBtc = Transaction.TransferBitcoin(2.0f, 10.0f, wallet);
            
            Assert.Multiple(() =>
            {
                Assert.That(depositBtc.AmountInBitcoin, Is.EqualTo(5.0f));
                Assert.That(depositBtc.AmountInEthereum, Is.EqualTo(25.0f));
                Assert.That(transferBtc.Destination, Is.SameAs(wallet));
            });
        }

        #endregion

        #region ==================== EQUIVALENCE PARTITIONING SUMMARY ====================

        /// <summary>
        /// EQUIVALENCE PARTITIONING pentru metoda Deposit:
        /// 
        /// +------------------+------------------------+------------------------+
        /// | Partition        | Values                 | Expected Result        |
        /// +------------------+------------------------+------------------------+
        /// | P1: Valid        | 0, 10, 100, 50000     | SUCCESS                |
        /// | P2: Negative     | -1, -10, -100         | NegativeDepositEx      |
        /// | P3: Not % 10     | 1, 9, 11, 3999        | DivisionDepositEx      |
        /// | P4: > 50000      | 50001, 50010, 100000  | TooMuchDepositEx       |
        /// +------------------+------------------------+------------------------+
        /// 
        /// BOUNDARY VALUES:
        /// - Lower bound: -1 (invalid), 0 (valid), 1 (invalid - not % 10)
        /// - Division bound: 9 (invalid), 10 (valid), 11 (invalid)
        /// - Upper bound: 49990 (valid), 50000 (valid), 50001 (invalid)
        /// </summary>
        [Test]
        [Category("Documentation")]
        public void EquivalencePartitioning_Documentation()
        {
            // Acest test serveste ca documentatie pentru partitionarea de echivalenta
            Assert.Pass("See summary above for equivalence partitioning documentation");
        }

        #endregion
    }
}
