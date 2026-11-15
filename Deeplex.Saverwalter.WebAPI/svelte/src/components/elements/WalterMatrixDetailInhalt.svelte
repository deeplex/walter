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
    Was für ein Feld einer Matrix schon eingetragen ist: Zusammenfassung und
    Einträge. Steht im Erfassen-Dialog über dem Formular.
-->
<script lang="ts">
    import {
        InlineLoading,
        StructuredList,
        StructuredListBody,
        StructuredListCell,
        StructuredListHead,
        StructuredListRow
    } from 'carbon-components-svelte';
    import type { MatrixDetail } from '$walter/services/matrix';

    export let detail: MatrixDetail;
    export let loading = false;
    export let leerText = 'Noch nichts eingetragen.';

    const betrag = (b: number) =>
        b.toLocaleString('de-DE', { style: 'currency', currency: 'EUR' });
    const datum = (d?: string) =>
        d
            ? new Date(d + 'T00:00:00').toLocaleDateString('de-DE', {
                  day: '2-digit',
                  month: '2-digit',
                  year: 'numeric'
              })
            : '';
</script>

<section class="vorhanden">
    <h5>Bereits eingetragen</h5>

    {#if loading}
        <InlineLoading description="Lade …" />
    {:else}
        {#if detail.zusammenfassung.length > 0}
            <dl class="zusammenfassung">
                {#each detail.zusammenfassung as z}
                    <div>
                        <dt>{z.label}</dt>
                        <dd>{z.wert}</dd>
                    </div>
                {/each}
            </dl>
        {/if}

        {#if detail.eintraege.length === 0}
            <p class="leer">{leerText}</p>
        {:else}
            <StructuredList condensed>
                <StructuredListHead>
                    <StructuredListRow head>
                        <StructuredListCell head>Datum</StructuredListCell>
                        <StructuredListCell head>Eintrag</StructuredListCell>
                        <StructuredListCell head
                            >{detail.wertLabel ?? 'Betrag'}</StructuredListCell
                        >
                    </StructuredListRow>
                </StructuredListHead>
                <StructuredListBody>
                    {#each detail.eintraege as e}
                        <StructuredListRow>
                            <StructuredListCell>
                                {datum(e.datum)}
                            </StructuredListCell>
                            <StructuredListCell>
                                {#if e.href}
                                    <a href={e.href}>{e.text}</a>
                                {:else}
                                    {e.text}
                                {/if}
                            </StructuredListCell>
                            <StructuredListCell>
                                {e.wert ?? betrag(e.betrag ?? 0)}
                            </StructuredListCell>
                        </StructuredListRow>
                    {/each}
                </StructuredListBody>
            </StructuredList>
        {/if}

        {#if detail.link}
            <p class="link">
                <a href={detail.link.href}>{detail.link.label}</a>
            </p>
        {/if}
    {/if}
</section>

<style>
    h5 {
        margin-bottom: 0.5rem;
    }

    .zusammenfassung {
        display: flex;
        gap: 2rem;
        margin-bottom: 1rem;
    }

    dt {
        opacity: 0.7;
        font-size: 0.875rem;
    }

    dd {
        font-weight: 600;
        margin: 0;
    }

    .leer {
        opacity: 0.7;
        margin: 0.5rem 0;
    }

    .link {
        margin-top: 0.75rem;
    }
</style>
