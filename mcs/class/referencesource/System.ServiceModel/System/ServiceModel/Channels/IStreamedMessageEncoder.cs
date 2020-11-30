﻿// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Channels
{
    using System.IO;

    interface IStreamedMessageEncoder
    {
        Stream GetResponseMessageStream(Message message);
    }
}
