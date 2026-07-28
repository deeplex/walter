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

<script lang="ts">
    import {
        Row,
        Column,
        Checkbox,
        TextInput,
        Button,
        Tag,
        InlineLoading,
        InlineNotification
    } from 'carbon-components-svelte';
    import { WalterToastContent } from '$walter/lib';
    import { walter_fetch, walter_post } from '$walter/services/requests';
    import { addToast } from '$walter/store';

    export let vertragId: number;
    export let fetchImpl: typeof fetch;

    type MietKorrekturMonat = {
        jahr: number;
        monat: number;
        istKaltmiete: number;
        sollKaltmiete: number;
        delta: number;
        status:
            | 'Anwendbar'
            | 'Abgerechnet'
            | 'NkReichtNichtAus'
            | 'BereitsKorrigiert';
    };

    const statusLabel: Record<MietKorrekturMonat['status'], string> = {
        Anwendbar: 'Anwendbar',
        Abgerechnet: 'Jahr bereits abgerechnet',
        NkReichtNichtAus: 'NK-Vorauszahlung reicht nicht aus',
        BereitsKorrigiert: 'Bereits korrigiert'
    };

    const statusTag: Record<
        MietKorrekturMonat['status'],
        'green' | 'gray' | 'red' | 'teal'
    > = {
        Anwendbar: 'green',
        Abgerechnet: 'gray',
        NkReichtNichtAus: 'red',
        BereitsKorrigiert: 'teal'
    };

    let monate: MietKorrekturMonat[] = [];
    let selected = new Set<string>();
    let grund = '';
    let loading = false;
    let anwendenLoading = false;

    const eur = (v: number) =>
        v.toLocaleString('de-DE', { style: 'currency', currency: 'EUR' });
    const monatText = (m: MietKorrekturMonat) =>
        `${String(m.monat).padStart(2, '0')}/${m.jahr}`;
    const key = (m: MietKorrekturMonat) => `${m.jahr}-${m.monat}`;

    export async function refresh() {
        if (!vertragId) return;
        loading = true;
        try {
            monate = (await (
                await walter_fetch(
                    fetchImpl,
                    `/api/vertraege/${vertragId}/miet-korrektur`
                )
            ).json()) as MietKorrekturMonat[];
            selected = new Set(
                [...selected].filter((k) => monate.some((m) => key(m) === k))
            );
        } finally {
            loading = false;
        }
    }

    $: vertragId, refresh();

    function toggle(m: MietKorrekturMonat) {
        const k = key(m);
        if (selected.has(k)) selected.delete(k);
        else selected.add(k);
        selected = selected;
    }

    $: anwendbareMonate = monate.filter((m) => m.status === 'Anwendbar');
    $: kannAnwenden = selected.size > 0 && grund.trim().length > 0;

    async function anwenden() {
        anwendenLoading = true;
        const toast = new WalterToastContent(
            'Miet-Korrektur gebucht',
            'Fehler bei der Miet-Korrektur',
            (...args: unknown[]) =>
                `${(args[0] as { korrigiert: unknown[] }).korrigiert.length} Monat(e) korrigiert.`,
            (e: unknown) => `Konnte Miet-Korrektur nicht buchen: ${e}`
        );
        try {
            const response = await walter_post(
                `/api/vertraege/${vertragId}/miet-korrektur`,
                {
                    monate: monate
                        .filter((m) => selected.has(key(m)))
                        .map((m) => ({ jahr: m.jahr, monat: m.monat })),
                    grund
                }
            );
            const parsed = await response.json();
            addToast(toast, response.status === 200, parsed);
            if (response.status === 200) {
                selected = new Set();
                grund = '';
                await refresh();
            }
        } finally {
            anwendenLoading = false;
        }
    }
</script>

{#if loading}
    <InlineLoading description="Prüfe Miet-Korrektur…" />
{:else if monate.length > 0}
    <InlineNotification
        kind="warning"
        title={`${anwendbareMonate.length || monate.length} Monat(e) mit abweichender Miete gefunden`}
        subtitle="Vermutlich wurde eine Mieterhöhung/-senkung rückwirkend nachgetragen — bereits gebuchte Kaltmiete/NK-Vorauszahlung stimmen dann nicht mehr mit der aktuellen Vertragsversion überein."
        hideCloseButton
    />
    <div style="overflow-x: auto; margin-top: 1rem;">
        <table class="bx--data-table" style="min-width: 40rem;">
            <thead>
                <tr>
                    <th />
                    <th>Monat</th>
                    <th>Gebuchte Kaltmiete</th>
                    <th>Soll-Kaltmiete</th>
                    <th>Differenz</th>
                    <th>Status</th>
                </tr>
            </thead>
            <tbody>
                {#each monate as m (key(m))}
                    <tr>
                        <td>
                            <Checkbox
                                hideLabel
                                labelText={`Monat ${monatText(m)} auswählen`}
                                disabled={m.status !== 'Anwendbar'}
                                checked={selected.has(key(m))}
                                on:change={() => toggle(m)}
                            />
                        </td>
                        <td>{monatText(m)}</td>
                        <td>{eur(m.istKaltmiete)}</td>
                        <td>{eur(m.sollKaltmiete)}</td>
                        <td>{eur(m.delta)}</td>
                        <td>
                            <Tag type={statusTag[m.status]}
                                >{statusLabel[m.status]}</Tag
                            >
                        </td>
                    </tr>
                {/each}
            </tbody>
        </table>
    </div>
    {#if anwendbareMonate.length > 0}
        <Row style="margin-top: 1rem; align-items: flex-end;">
            <Column>
                <TextInput
                    required
                    labelText="Grund"
                    placeholder="z.B. rückwirkende Mieterhöhung ab 11/2025"
                    bind:value={grund}
                />
            </Column>
            <Column>
                <Button disabled={!kannAnwenden || anwendenLoading} on:click={anwenden}>
                    {anwendenLoading
                        ? 'Wird gebucht…'
                        : `${selected.size || ''} Monat(e) korrigieren`}
                </Button>
            </Column>
        </Row>
    {/if}
{/if}
