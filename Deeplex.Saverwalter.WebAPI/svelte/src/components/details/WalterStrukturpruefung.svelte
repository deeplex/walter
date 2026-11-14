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
    Strukturprüfung: was die Annahmen der Fachsicht verletzt — Konten mit mehreren Besitzern
    (ihre Buchungen lassen sich nicht zuordnen) und Zahlungen ohne Zuordnung (Bestand, bei
    dem nicht festgehalten wurde, wofür gezahlt wurde). Stornierte Zahlungen ohne Zuordnung
    stehen nur zur Information da.
-->
<script lang="ts">
    import type { WalterEntityRef } from '$walter/lib/WalterEntityRef';
    import { formatEuro, formatDatum } from '$walter/services/format';
    import {
        InlineLoading,
        InlineNotification,
        Tag,
        Tile
    } from 'carbon-components-svelte';
    import { walter_fetch } from '$walter/services/requests';

    export let fetchImpl: typeof fetch;

    type MehrdeutigesKonto = {
        kontoId: number;
        kontonummer: string;
        bezeichnung: string;
        besitzer: string[];
    };
    type ZahlungOhneZuordnung = {
        transaktionId: string;
        datum: string;
        betrag: number;
        text: string;
        ref?: WalterEntityRef | null;
        storniert: boolean;
    };
    type Ergebnis = {
        mehrdeutigeKonten: MehrdeutigesKonto[];
        zahlungenOhneZuordnung: ZahlungOhneZuordnung[];
        auffaellig: boolean;
    };

    let ergebnis: Ergebnis | undefined;
    let fehler: string | null = null;

    async function lade() {
        const r = await walter_fetch(
            fetchImpl,
            '/api/jahresabschluss/struktur'
        );
        if (r.ok) ergebnis = (await r.json()) as Ergebnis;
        else fehler = await r.text();
    }
    lade();

    $: offen =
        ergebnis?.zahlungenOhneZuordnung.filter((z) => !z.storniert) ?? [];
    $: storniert =
        ergebnis?.zahlungenOhneZuordnung.filter((z) => z.storniert) ?? [];
    $: anzahl = (ergebnis?.mehrdeutigeKonten.length ?? 0) + offen.length;
</script>

<Tile id="strukturpruefung" style="margin-bottom: 1rem">
    <h4 style="margin-bottom: 0.75rem">
        Strukturprüfung
        {#if ergebnis}
            {#if anzahl > 0}
                <Tag type="red"
                    >{anzahl} Auffälligkeit{anzahl === 1 ? '' : 'en'}</Tag
                >
            {:else}
                <Tag type="green">Keine Auffälligkeiten</Tag>
            {/if}
        {/if}
    </h4>

    {#if fehler}
        <InlineNotification
            kind="error"
            title="Strukturprüfung nicht verfügbar"
            subtitle={fehler}
            hideCloseButton
        />
    {:else if !ergebnis}
        <InlineLoading description="Prüfe…" />
    {:else}
        {#if ergebnis.mehrdeutigeKonten.length > 0}
            <h5>Konten mit mehreren Besitzern</h5>
            <p class="hinweis">
                Buchungen auf diesen Konten lassen sich keinem Fachobjekt
                zuordnen.
            </p>
            <ul class="liste">
                {#each ergebnis.mehrdeutigeKonten as k (k.kontoId)}
                    <li>
                        <a href={`/buchungskonten/${k.kontoId}`}
                            >{k.kontonummer} {k.bezeichnung}</a
                        >
                        <span>{k.besitzer.join(', ')}</span>
                    </li>
                {/each}
            </ul>
        {/if}
        {#if offen.length > 0}
            <h5>Zahlungen ohne Zuordnung</h5>
            <p class="hinweis">
                Nicht festgehalten, für welchen Posten gezahlt wurde — die
                Zahlung ist keinem Monat, keiner Abrechnung bzw. Rechnung
                zugeordnet und gleicht nichts aus. Bitte prüfen und
                gegebenenfalls rückgängig machen und neu erfassen.
            </p>
            <ul class="liste">
                {#each offen as z (z.transaktionId + z.text)}
                    <li>
                        <span>{formatDatum(z.datum)}</span>
                        <span>{formatEuro(z.betrag)}</span>
                        <a href={`/transaktionen/${z.transaktionId}`}
                            >{z.text}</a
                        >
                    </li>
                {/each}
            </ul>
        {/if}
        {#if storniert.length > 0}
            <h5>Rückgängig gemachte Zahlungen ohne Zuordnung</h5>
            <p class="hinweis">
                Nur zur Information: Monat bzw. Jahr sind nicht bekannt.
            </p>
            <ul class="liste">
                {#each storniert as z (z.transaktionId + z.text)}
                    <li>
                        <span>{formatDatum(z.datum)}</span>
                        <span>{formatEuro(z.betrag)}</span>
                        <a href={`/transaktionen/${z.transaktionId}`}
                            >{z.text}</a
                        >
                    </li>
                {/each}
            </ul>
        {/if}
        {#if anzahl === 0 && storniert.length === 0}
            <p>
                Alle Konten haben genau einen Besitzer, alle Zahlungen sind
                zugeordnet.
            </p>
        {/if}
    {/if}
</Tile>

<style>
    h5 {
        margin-top: 1rem;
    }
    .hinweis {
        color: var(--cds-text-secondary);
        margin-bottom: 0.5rem;
    }
    .liste {
        list-style: none;
        margin: 0;
        padding: 0;
    }
    .liste li {
        display: flex;
        flex-wrap: wrap;
        gap: 1rem;
        padding: 0.25rem 0;
    }
</style>
