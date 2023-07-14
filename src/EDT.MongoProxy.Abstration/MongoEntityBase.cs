﻿using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace EDT.MongoProxy.Abstration;

public abstract class MongoEntityBase
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public virtual string Id { get; set; }
}