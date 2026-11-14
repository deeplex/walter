<!-- Copyright (C) 2023-2026  Kai Lawrence -->
<!--
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program. If not, see <https://www.gnu.org/licenses/>.
-->

<!--
    Zahlungsübersicht: was war fällig, was kam, was ist offen. Eine Zeile je Periode
    bzw. Forderung in einer aufklappbaren WalterDataTable; aufgeklappt die Aufschlüsselung
    und die einzelnen Zahlungen als WalterEintraege.
-->
<script lang="ts">
    import { formatEuro, formatEuroOderStrich } from '$walter/services/format';
    import { Add } from 'carbon-icons-svelte';
    import WalterDataTable from '../elements/WalterDataTable.svelte';
    import WalterEintraege from '../elements/WalterEintraege.svelte';
    import WalterRueckgaengig from '../details/WalterRueckgaengig.svelte';
    import {
        zahlungsStatus,
        type WalterEintrag,
        type WalterZahlungsZeile
    } from '$walter/lib';
    import { createEventDispatcher } from 'svelte';

    export let rows: WalterZahlungsZeile[] = [];
    /** Leere Liste: dieser Text statt der Tabelle. */
    export let leerText = 'Keine Forderungen vorhanden.';
    /** „+ Zahlung“ bei Zeilen mit `erfassenFuer` und „Rückgängig machen“ je Zahlung. */
    export let bearbeitbar = false;
    export let fetchImpl: typeof fetch = fetch;
    /** Spaltenköpfe für Zeitraum und Fällig, z. B. „Rechnung“ und „Betrag“. */
    export let periodeTitel = 'Zeitraum';
    export let faelligTitel = 'Fällig';
    /** Mit Titel: Zusatzspalte aus `zusatz` der Zeilen (z. B. „Auf Mieter verteilt“). */
    export let zusatzTitel: string | undefined = undefined;
    /**
     * Mietkonto: statt „Bezahlt“ die Transaktionen hinter der Zeile (ganze Beträge), mit
     * `mitNk` dazu deren NK-Vorauszahlungs-Anteil.
     */
    export let mitTransaktionen = false;
    export let mitNk = false;
    /** Zeilen je Seite, z. B. 12 Monate im Mietkonto. */
    export let pageSize = 25;

    /** `zahlung`: „+ Zahlung“ geklickt; `geaendert`: eine Zahlung wurde rückgängig gemacht. */
    const dispatch = createEventDispatcher<{
        zahlung: WalterZahlungsZeile;
        geaendert: void;
    }>();

    // Beträge sind vorformatiert; sortiert wird in dieser Übersicht nicht.
    $: headers = [
        { key: 'periode', value: periodeTitel },
        { key: 'faellig', value: faelligTitel },
        ...(mitTransaktionen
            ? [
                  { key: 'transaktionen', value: 'Transaktion' },
                  ...(mitNk
                      ? [{ key: 'nkVorauszahlung', value: 'davon NK-VZ' }]
                      : [])
              ]
            : [{ key: 'bezahlt', value: 'Bezahlt' }]),
        { key: 'offen', value: 'Offen' },
        { key: 'status', value: 'Status' },
        ...(zusatzTitel ? [{ key: 'zusatz', value: zusatzTitel }] : []),
        ...(bearbeitbar ? [{ key: 'button', value: '' }] : [])
    ].map((h) => ({ ...h, sort: false as const }));

    $: zeilen = new Map(rows.map((z) => [z.id, z]));

    $: tableRows = rows.map((z) => ({
        id: z.id,
        periode: z.periode,
        faellig: formatEuro(z.faellig),
        bezahlt: formatEuro(z.bezahlt),
        transaktionen: formatEuroOderStrich(z.transaktionen),
        nkVorauszahlung: formatEuroOderStrich(z.nkVorauszahlung),
        offen: formatEuroOderStrich(z.offen),
        status: z.status ?? zahlungsStatus(z.faellig, z.bezahlt),
        zusatz: z.zusatz ?? '',
        button:
            bearbeitbar && z.erfassenFuer
                ? {
                      icon: Add,
                      kind: 'ghost' as const,
                      iconDescription: 'Zahlung erfassen',
                      onClick: () => dispatch('zahlung', z)
                  }
                : null
    }));

    $: nichtAufklappbar = rows
        .filter(
            (r) =>
                r.zahlungen.length === 0 &&
                r.aufschluesselung.length === 0 &&
                !r.abschnitte?.length
        )
        .map((r) => r.id);

    function aufschluesselung(z: WalterZahlungsZeile): WalterEintrag[] {
        return z.aufschluesselung.map((teil, i) => ({
            id: `teil-${i}`,
            text: `${teil.text}: fällig ${formatEuro(teil.faellig)}, bezahlt ${formatEuro(teil.bezahlt)}`
        }));
    }
</script>

{#if rows.length === 0}
    <p>{leerText}</p>
{:else}
    <WalterDataTable
        size="short"
        search={false}
        {pageSize}
        expandable
        nonExpandableRowIds={nichtAufklappbar}
        {headers}
        rows={tableRows}
        rowHref={(row) => zeilen.get(`${row.id}`)?.href}
    >
        <svelte:fragment slot="expanded-row" let:row>
            {@const zeile = zeilen.get(`${row.id}`)}
            {#if zeile}
                <WalterEintraege eintraege={aufschluesselung(zeile)} />
                {#each zeile.abschnitte ?? [] as abschnitt}
                    <h6 class="abschnitt">{abschnitt.titel}</h6>
                    <WalterEintraege eintraege={abschnitt.eintraege} />
                {/each}
                {#if zeile.abschnitte?.length}
                    <h6 class="abschnitt">Zahlungen</h6>
                {/if}
                <WalterEintraege
                    eintraege={zeile.zahlungen}
                    leerText="Noch keine Zahlung."
                >
                    <svelte:fragment slot="aktion" let:eintrag>
                        {#if bearbeitbar}
                            <WalterRueckgaengig
                                {fetchImpl}
                                transaktionId={eintrag.transaktionId}
                                buchungssatzId={eintrag.buchungssatzId}
                                on:erledigt={() => dispatch('geaendert')}
                            />
                        {/if}
                    </svelte:fragment>
                </WalterEintraege>
            {/if}
        </svelte:fragment>
    </WalterDataTable>
{/if}

<style>
    .abschnitt {
        margin: 0.75rem 0 0.25rem 0;
        color: var(--cds-text-02);
    }
    .abschnitt:first-child {
        margin-top: 0.25rem;
    }
</style>
