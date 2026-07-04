import type {
  Person,
  CreatePersonRequest,
  Transaction,
  CreateTransactionRequest,
  Totals,
  ProblemDetails,
} from "./types";

// API base URL. Can be overridden with a Vite environment variable
// (VITE_API_URL); defaults to the local back-end.
const BASE_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5000/api";

// Typed API error: carries the user-friendly message already extracted from the
// Problem Details payload plus the HTTP status, for components to display.
export class ApiError extends Error {
  status: number;
  constructor(message: string, status: number) {
    super(message);
    this.name = "ApiError";
    this.status = status;
  }
}

// Turns a Problem Details payload into a readable message:
// 1) use 'detail' (business rules / 404); otherwise
// 2) join the messages in 'errors' (field validation); otherwise
// 3) fall back to 'title'.
function extractMessage(problem: ProblemDetails | null, status: number): string {
  if (problem?.detail) return problem.detail;

  if (problem?.errors) {
    const messages = Object.values(problem.errors).flat();
    if (messages.length > 0) return messages.join(" ");
  }

  if (problem?.title) return problem.title;
  return `Erro inesperado (HTTP ${status}).`;
}

// Central fetch wrapper: sets the Content-Type, converts error responses into an
// ApiError and deserializes the JSON body when present.
async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const resp = await fetch(`${BASE_URL}${path}`, {
    headers: { "Content-Type": "application/json" },
    ...options,
  });

  // On error, try to read the body as Problem Details and throw an ApiError.
  if (!resp.ok) {
    let problem: ProblemDetails | null = null;
    try {
      problem = await resp.json();
    } catch {
      // empty or non-JSON body: proceed with the status only
    }
    throw new ApiError(extractMessage(problem, resp.status), resp.status);
  }

  // 204 No Content (e.g. DELETE) has no body to deserialize.
  if (resp.status === 204) return undefined as T;

  return (await resp.json()) as T;
}

// ----- API functions, one per endpoint -----

export const api = {
  // People
  listPeople: () => request<Person[]>("/people"),
  createPerson: (body: CreatePersonRequest) =>
    request<Person>("/people", { method: "POST", body: JSON.stringify(body) }),
  deletePerson: (id: string) =>
    request<void>(`/people/${id}`, { method: "DELETE" }),

  // Transactions
  listTransactions: () => request<Transaction[]>("/transactions"),
  createTransaction: (body: CreateTransactionRequest) =>
    request<Transaction>("/transactions", { method: "POST", body: JSON.stringify(body) }),

  // Totals
  getTotals: () => request<Totals>("/totals"),
};
