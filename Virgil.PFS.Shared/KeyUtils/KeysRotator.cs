using System;
using System.Collections.Generic;
using System.Text;
using Virgil.SDK.Client;
using Virgil.SDK.Cryptography;

namespace Virgil.PFS.Shared.KeyUtils
{
    internal class KeysRotator
    {
        private EphemeralCardManager cardManager;
        private CardModel identityCard;
        private readonly IPrivateKey privateKey;

        public KeysRotator(CardModel myIdentityCard, IPrivateKey privateKey, EphemeralCardManager ephemeralCardManager)
        {
            this.cardManager = ephemeralCardManager;
            this.privateKey = privateKey;
            this.identityCard = myIdentityCard;
        }

        public async void Rotate(int desireNumberOfCards)
        {
            var numberOfOtCard = await this.cardManager.GetOtCardsCount(this.identityCard.Id);
            var missingCards = ((desireNumberOfCards - numberOfOtCard.Active) > 0)
                ? (desireNumberOfCards - numberOfOtCard.Active) : 0;
            await cardManager.BootstrapCardsSet(
                this.identityCard,
                this.privateKey,
                missingCards
                );
        }
    }


}
