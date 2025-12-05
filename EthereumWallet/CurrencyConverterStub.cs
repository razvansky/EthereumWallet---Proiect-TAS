using System;
using System.Collections.Generic;
using System.Text;

namespace EthereumWallet
{
    public class CurrencyConverterStub : ICurrencyConverterStub
    {
        private readonly float rateBitcoinEthereum;
        public CurrencyConverterStub(float rateBitcoinEthereum)
        {
            if (rateBitcoinEthereum <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rateBitcoinEthereum), "Rate must be positive.");
            }
            this.rateBitcoinEthereum = rateBitcoinEthereum;
        }

        public float BitcoinToEthereum(float ValueInBitcoin)
        {
            if (ValueInBitcoin < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(ValueInBitcoin), "Amount must be non-negative.");
            }
            return ValueInBitcoin * rateBitcoinEthereum;
        }

        public float EthereumToBitcoin(float ValueInEthereum)
        {
            if (ValueInEthereum < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(ValueInEthereum), "Amount must be non-negative.");
            }
            return ValueInEthereum / rateBitcoinEthereum;
        }
    }
}
