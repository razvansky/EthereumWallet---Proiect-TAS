namespace EthereumWallet
{
    public interface ICurrencyConverterStub
    {
        float BitcoinToEthereum(float ValueInBitcoin);
        float EthereumToBitcoin(float ValueInEthereum);
    }
}