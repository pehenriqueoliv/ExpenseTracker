// Formats a number as Brazilian currency (R$ 1.234,56).
export function formatCurrency(value: number): string {
  return value.toLocaleString("pt-BR", {
    style: "currency",
    currency: "BRL",
  });
}

// Maps a transaction type to its Portuguese label used in the UI.
export function transactionTypeLabel(type: "Expense" | "Income"): string {
  return type === "Income" ? "Receita" : "Despesa";
}
