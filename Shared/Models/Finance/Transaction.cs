﻿using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace hub.Shared.Models.Finance;

[PrimaryKey(nameof(Id))]
[Index(nameof(PostingDate))]
[Index(nameof(PostingDate), nameof(Description))]
public class Transaction
{
	public int Id { get; set; }
	public DateTime PostingDate { get; set; }
	[MaxLength(128)]
	public string Description { get; set; }
	public decimal Amount { get; set; }
	public decimal Balance { get; set; }
	public Category Category { get; set; }

	public static Transaction TryParse(string line)
	{
		var items = line.Split(',');

		if (items.Length != 8)
		{
			return null;
		}

		if (!DateTime.TryParse(items[1], out var time))
		{
			return null;
		}

		if (!decimal.TryParse(items[3], out var amount))
		{
			return null;
		}
		
		if (!decimal.TryParse(items[5], out var balance))
		{
			return null;
		}

		return new Transaction
			{
				Amount = amount,
				Description = items[2].Length > 128 ? items[2][..128] : items[2],
				PostingDate = time,
				Balance = balance
			}
		;
	}
}