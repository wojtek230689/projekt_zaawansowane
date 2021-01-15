﻿using System;
using System.Collections.Generic;
using System.Text;

namespace PROJEKT.Classes.Exceptions
{
    public class MessageFactoryIfaceNotFound : Exception
    {
        public MessageFactoryIfaceNotFound(string a_sTypeName) :
            base($"Klasa <{a_sTypeName}> nie implementuje interfejsu IMessage!")
        {
        }
    }
}
