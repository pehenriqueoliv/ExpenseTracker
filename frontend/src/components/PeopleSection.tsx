import { useState } from "react";
import { api, ApiError } from "../api/client";
import type { Person } from "../api/types";

// The people list comes from App (shared state with the transaction dropdown);
// 'onChanged' asks App to reload the dependent lists after create/delete.
interface Props {
  people: Person[];
  // True while the people list is being fetched (shown as a loading state).
  loading: boolean;
  onChanged: () => Promise<void> | void;
}

// Person registration + listing section.
export function PeopleSection({ people, loading, onChanged }: Props) {
  // Controlled form fields.
  const [name, setName] = useState("");
  const [age, setAge] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setSaving(true);
    try {
      // Convert age (input string) to a number before sending.
      await api.createPerson({ name, age: Number(age) });
      // Clear the form and ask App to reload the dependent lists.
      setName("");
      setAge("");
      await onChanged();
    } catch (err) {
      // Show the message returned by the API (e.g. name/age validation).
      setError(err instanceof ApiError ? err.message : "Falha ao criar pessoa.");
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete(person: Person) {
    // Deleting a person also removes their transactions (cascade on the back-end),
    // so confirm first to avoid accidental data loss.
    const confirmed = window.confirm(
      `Deletar "${person.name}"? Isso também apaga todas as transações dessa pessoa.`
    );
    if (!confirmed) return;

    setError(null);
    try {
      await api.deletePerson(person.id);
      await onChanged();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Falha ao deletar pessoa.");
    }
  }

  return (
    <section className="card">
      <h2>Pessoas</h2>

      <form className="form" onSubmit={handleSubmit}>
        <div className="field">
          <label htmlFor="person-name">Nome</label>
          <input
            id="person-name"
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="Ex.: Maria"
            required
          />
        </div>
        <div className="field field-age">
          <label htmlFor="person-age">Idade</label>
          <input
            id="person-age"
            type="number"
            min={0}
            max={130}
            value={age}
            onChange={(e) => setAge(e.target.value)}
            placeholder="Ex.: 30"
            required
          />
        </div>
        <button type="submit" disabled={saving}>
          {saving ? "Salvando..." : "Adicionar"}
        </button>
      </form>

      {/* API error message, if any */}
      {error && <p className="error-box">{error}</p>}

      {loading && people.length === 0 ? (
        <p className="empty-state">Carregando pessoas...</p>
      ) : people.length === 0 ? (
        <p className="empty-state">Nenhuma pessoa cadastrada ainda.</p>
      ) : (
        <table className="data-table">
          <thead>
            <tr>
              <th>Nome</th>
              <th>Idade</th>
              <th aria-label="Ações"></th>
            </tr>
          </thead>
          <tbody>
            {people.map((p) => (
              <tr key={p.id}>
                <td>{p.name}</td>
                <td>{p.age}</td>
                <td className="col-action">
                  <button
                    className="btn-danger"
                    onClick={() => handleDelete(p)}
                    title="Deletar pessoa e suas transações"
                  >
                    Deletar
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </section>
  );
}
