using System;
using System.Collections.Generic;

namespace EthereumWallet
{
    /// <summary>
    /// =============================================================================
    /// TEST DOUBLES - Toate tipurile de obiecte substitut pentru testare
    /// =============================================================================
    /// 
    /// 1. STUB - Implementare simplificat? cu valori hardcodate (CurrencyConverterStub.cs)
    /// 2. MOCK - Obiect simulat cu Moq pentru verificare interac?iuni (în UnitTesting.cs)
    /// 3. FAKE - Implementare func?ional? simplificat? (mai jos)
    /// 4. SPY - Wrapper care înregistreaz? apelurile (mai jos)
    /// 5. DUMMY - Obiect care nu face nimic, doar ocup? loc (mai jos)
    /// </summary>

    #region FAKE - Implementare func?ional? simplificat?

    /// <summary>
    /// FAKE: Implementare complet? dar simplificat? a convertorului.
    /// Folose?te un dic?ionar intern pentru rate de conversie.
    /// Diferen?a fa?? de Stub: Fake are logic? real?, Stub returneaz? valori fixe.
    /// </summary>
    public class FakeCurrencyConverter : ICurrencyConverterStub
    {
        private readonly Dictionary<string, float> _exchangeRates;
        private readonly List<string> _conversionHistory;

        public FakeCurrencyConverter()
        {
            // Rate de schimb "fake" dar realiste
            _exchangeRates = new Dictionary<string, float>
            {
                { "BTC_ETH", 15.5f },   // 1 BTC = 15.5 ETH
                { "ETH_BTC", 0.0645f }  // 1 ETH = 0.0645 BTC
            };
            _conversionHistory = new List<string>();
        }

        public FakeCurrencyConverter(float btcToEthRate)
        {
            _exchangeRates = new Dictionary<string, float>
            {
                { "BTC_ETH", btcToEthRate },
                { "ETH_BTC", 1f / btcToEthRate }
            };
            _conversionHistory = new List<string>();
        }

        public float BitcoinToEthereum(float ValueInBitcoin)
        {
            if (ValueInBitcoin < 0)
                throw new ArgumentOutOfRangeException(nameof(ValueInBitcoin));

            var result = ValueInBitcoin * _exchangeRates["BTC_ETH"];
            _conversionHistory.Add($"BTC->ETH: {ValueInBitcoin} BTC = {result} ETH");
            return result;
        }

        public float EthereumToBitcoin(float ValueInEthereum)
        {
            if (ValueInEthereum < 0)
                throw new ArgumentOutOfRangeException(nameof(ValueInEthereum));

            var result = ValueInEthereum * _exchangeRates["ETH_BTC"];
            _conversionHistory.Add($"ETH->BTC: {ValueInEthereum} ETH = {result} BTC");
            return result;
        }

        public IReadOnlyList<string> GetConversionHistory() => _conversionHistory.AsReadOnly();

        public void SetExchangeRate(string pair, float rate)
        {
            _exchangeRates[pair] = rate;
        }
    }

    #endregion

    #region SPY - Inregistreaza toate apelurile pentru verificare

    /// <summary>
    /// SPY: Wrapper care inregistreaza toate apelurile si parametrii.
    /// Diferenta fata de Mock: Spy-ul foloseste implementarea reala si doar inregistreaza.
    /// </summary>
    public class SpyCurrencyConverter : ICurrencyConverterStub
    {
        private readonly ICurrencyConverterStub _realConverter;
        private readonly List<ConversionCall> _calls;

        public SpyCurrencyConverter(ICurrencyConverterStub realConverter)
        {
            _realConverter = realConverter ?? throw new ArgumentNullException(nameof(realConverter));
            _calls = new List<ConversionCall>();
        }

        public float BitcoinToEthereum(float ValueInBitcoin)
        {
            var result = _realConverter.BitcoinToEthereum(ValueInBitcoin);
            _calls.Add(new ConversionCall
            {
                MethodName = nameof(BitcoinToEthereum),
                Input = ValueInBitcoin,
                Output = result,
                Timestamp = DateTime.UtcNow
            });
            return result;
        }

        public float EthereumToBitcoin(float ValueInEthereum)
        {
            var result = _realConverter.EthereumToBitcoin(ValueInEthereum);
            _calls.Add(new ConversionCall
            {
                MethodName = nameof(EthereumToBitcoin),
                Input = ValueInEthereum,
                Output = result,
                Timestamp = DateTime.UtcNow
            });
            return result;
        }

        public int TotalCalls => _calls.Count;
        public int BitcoinToEthereumCallCount => _calls.FindAll(c => c.MethodName == nameof(BitcoinToEthereum)).Count;
        public int EthereumToBitcoinCallCount => _calls.FindAll(c => c.MethodName == nameof(EthereumToBitcoin)).Count;
        public IReadOnlyList<ConversionCall> GetAllCalls() => _calls.AsReadOnly();

        public bool WasCalledWith(string methodName, float input)
        {
            return _calls.Exists(c => c.MethodName == methodName && Math.Abs(c.Input - input) < 0.001f);
        }

        public bool WasNeverCalled() => _calls.Count == 0;
    }

    public class ConversionCall
    {
        public string MethodName { get; set; } = string.Empty;
        public float Input { get; set; }
        public float Output { get; set; }
        public DateTime Timestamp { get; set; }
    }

    #endregion

    #region DUMMY - Obiect placeholder care nu face nimic util

    /// <summary>
    /// DUMMY: Obiect care nu face nimic, doar ocupa loc cand trebuie sa pasam un parametru
    /// dar nu ne intereseaza comportamentul lui.
    /// </summary>
    public class DummyCurrencyConverter : ICurrencyConverterStub
    {
        public float BitcoinToEthereum(float ValueInBitcoin)
        {
            return 0;
        }

        public float EthereumToBitcoin(float ValueInEthereum)
        {
            return 0;
        }
    }

    #endregion

    #region FAKE Wallet Repository - pentru testarea persistentei

    public interface IWalletRepository
    {
        void Save(Wallet wallet);
        Wallet? GetById(Guid id);
        IEnumerable<Wallet> GetAll();
        void Delete(Guid id);
        bool Exists(Guid id);
    }

    /// <summary>
    /// FAKE Repository: Implementare in-memory pentru teste.
    /// Nu foloseste baza de date reala, dar are comportament complet functional.
    /// </summary>
    public class FakeWalletRepository : IWalletRepository
    {
        private readonly Dictionary<Guid, Wallet> _wallets = new Dictionary<Guid, Wallet>();
        private readonly Dictionary<Wallet, Guid> _walletIds = new Dictionary<Wallet, Guid>();

        public void Save(Wallet wallet)
        {
            if (wallet == null) throw new ArgumentNullException(nameof(wallet));

            if (!_walletIds.ContainsKey(wallet))
            {
                var id = Guid.NewGuid();
                _walletIds[wallet] = id;
                _wallets[id] = wallet;
            }
        }

        public Wallet? GetById(Guid id)
        {
            return _wallets.TryGetValue(id, out var wallet) ? wallet : null;
        }

        public IEnumerable<Wallet> GetAll()
        {
            return _wallets.Values;
        }

        public void Delete(Guid id)
        {
            if (_wallets.TryGetValue(id, out var wallet))
            {
                _walletIds.Remove(wallet);
                _wallets.Remove(id);
            }
        }

        public bool Exists(Guid id)
        {
            return _wallets.ContainsKey(id);
        }

        public Guid GetWalletId(Wallet wallet)
        {
            return _walletIds.TryGetValue(wallet, out var id) ? id : Guid.Empty;
        }

        public int Count => _wallets.Count;

        public void Clear()
        {
            _wallets.Clear();
            _walletIds.Clear();
        }
    }

    #endregion
}
