using System;
using UnityEngine;

public class Countable<U>
{
    public U Data { get; private set; }
    public int Stock { get; private set; }

    public Countable(U data, int stock) {
        this.Data = data;
        this.Stock = stock;
    }

    public void AddToStock(int quantity) {
        this.Stock += quantity;
        Debug.Log($"New stock {Stock}");
    }

    public bool RemoveStock(int quantity) {
        this.Stock = Math.Max(this.Stock - quantity, 0);
        return this.Stock == 0;
    }

    public Countable<U> GetCopy() {
        return new Countable<U>(Data, Stock);
    }

    public override bool Equals(object obj) {
        if (obj is Countable<U>) {
            Countable<U> countable = (Countable<U>)obj;
            return countable.Stock == Stock && countable.Data.Equals(Data);
        }

        return false;
    }

    public override int GetHashCode() {
        return HashCode.Combine(Data, Stock);
    }

    public static bool operator ==(Countable<U> a, Countable<U> b) {
        return a.Equals(b);
    }

    public static bool operator !=(Countable<U> a, Countable<U> b) {
        return !a.Equals(b);
    }
}

