import { useCallback, useEffect, useState } from "react";
import { api, ApiError } from "./api/client";
import type { Person } from "./api/types";
import { PeopleSection } from "./components/PeopleSection";
import { TransactionsSection } from "./components/TransactionsSection";
import { TotalsSection } from "./components/TotalsSection";
import "./App.css";

export default function App() {
  // People list kept in App: shared between the People section (table) and the
  // Transactions section (person dropdown).
  const [people, setPeople] = useState<Person[]>([]);

  // Version counter: when it changes, the Transactions and Totals sections
  // reload. A simple way to keep the sections in sync without a global store.
  const [version, setVersion] = useState(0);

  const [globalError, setGlobalError] = useState<string | null>(null);

  // Loads the people list from the API.
  const loadPeople = useCallback(async () => {
    try {
      setPeople(await api.listPeople());
    } catch (err) {
      setGlobalError(
        err instanceof ApiError ? err.message : "Falha ao carregar pessoas."
      );
    }
  }, []);

  // Called after any change (create/delete person or create transaction):
  // reloads the people and bumps the version to refresh the other sections.
  const handleChanged = useCallback(async () => {
    await loadPeople();
    setVersion((v) => v + 1);
  }, [loadPeople]);

  // Initial load when the app opens.
  useEffect(() => {
    loadPeople();
  }, [loadPeople]);

  return (
    <div className="container">
      <header className="page-header">
        <h1>Controle de Gastos Residenciais</h1>
        <p>Cadastre pessoas, lance receitas e despesas e acompanhe os totais.</p>
      </header>

      {globalError && <p className="error-box">{globalError}</p>}

      <main className="grid">
        <PeopleSection people={people} onChanged={handleChanged} />
        <TransactionsSection people={people} version={version} onChanged={handleChanged} />
        <TotalsSection version={version} />
      </main>
    </div>
  );
}
