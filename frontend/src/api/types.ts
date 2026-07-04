// TypeScript types that mirror the back-end DTOs, giving the front-end a fully
// typed contract: the compiler flags any mismatched field.

// Transaction type. The API serializes it as text ("Expense"/"Income"), so a
// string union is the simplest, safest representation on the TypeScript side.
export type TransactionType = "Expense" | "Income";

// ----- Person -----

// Mirrors PersonResponse.
export interface Person {
  id: string; // Guid serialized as string
  name: string;
  age: number;
}

// Mirrors CreatePersonRequest.
export interface CreatePersonRequest {
  name: string;
  age: number;
}

// ----- Transaction -----

// Mirrors TransactionResponse.
export interface Transaction {
  id: string;
  description: string;
  amount: number;
  type: TransactionType;
  personId: string;
}

// Mirrors CreateTransactionRequest.
export interface CreateTransactionRequest {
  description: string;
  amount: number;
  type: TransactionType;
  personId: string;
}

// ----- Totals -----

// Mirrors PersonTotalResponse.
export interface PersonTotal {
  personId: string;
  name: string;
  totalIncome: number;
  totalExpenses: number;
  balance: number;
}

// Mirrors TotalsResponse.
export interface Totals {
  people: PersonTotal[];
  overallTotalIncome: number;
  overallTotalExpenses: number;
  overallBalance: number;
}

// ----- Error (Problem Details / RFC 7807) -----

// Error shape returned by the API. 'detail' carries the business-rule message;
// 'errors' carries field validation failures (Data Annotations).
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  errors?: Record<string, string[]>;
}
