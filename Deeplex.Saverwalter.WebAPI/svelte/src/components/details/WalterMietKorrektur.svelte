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
    import { formatEuro, formatDate } from '$walter/services/format';
    import {
        Row,
        Column,
        TextInput,
        Button,
        Tag,
        InlineLoading,
        InlineNotification,
        Modal
    } from 'carbon-components-svelte';
    import { WalterToastContent } from '$walter/lib';
    import {
        walter_delete,
        walter_fetch,
        walter_post
    } from '$walter/services/requests';
    import { addToast } from '$walter/store';
    import { createEventDispatcher } from 'svelte';

    /** `geaendert`: eine Korrektur wurde gebucht, gelöscht oder storniert. */
    const dispatch = createEventDispatcher<{ geaendert: void }>();

    export let vertragId: number;
    export let fetchImpl: typeof fetch;

    type Status = 'Stimmt' | 'Anwendbar' | 'Abgerechnet' | 'NkReichtNichtAus';

    type MietKorrekturMonat = {
        monat: number;
        istKaltmiete: number;
        sollKaltmiete: number;
        delta: number;
    };

    type MietKorrekturBuchung = {
        id: string;
        datum: string;
        betrag: number;
        beschreibung: string;
        notiz: string | null;
        kannLoeschen: boolean;
        kannStornieren: boolean;
        sperrgrund: string | null;
    };

    type MietKorrekturJahr = {
        jahr: number;
        monate: MietKorrekturMonat[];
        korrekturen: MietKorrekturBuchung[];
        abweichung: number;
        korrigiert: number;
        differenz: number;
        nkVorauszahlung: number;
        status: Status;
    };

    const statusLabel: Record<Status, string> = {
        Stimmt: 'Stimmt mit dem Vertrag überein',
        Anwendbar: 'Differenz offen',
        Abgerechnet: 'Jahr bereits abgerechnet',
        NkReichtNichtAus: 'NK-Vorauszahlung reicht nicht aus'
    };

    const statusTag: Record<Status, 'green' | 'gray' | 'red' | 'teal'> = {
        Stimmt: 'teal',
        Anwendbar: 'green',
        Abgerechnet: 'gray',
        NkReichtNichtAus: 'red'
    };

    let jahre: MietKorrekturJahr[] = [];
    let grund = '';
    let loading = false;
    let busy = false;
    let fehler: string | null = null;
    let stornoBuchung: MietKorrekturBuchung | null = null;
    let stornoGrund = '';

    const monatText = (jahr: number, m: MietKorrekturMonat) =>
        `${String(m.monat).padStart(2, '0')}/${jahr}`;

    export async function refresh() {
        if (!vertragId) return;
        loading = true;
        try {
            jahre = (await (
                await walter_fetch(
                    fetchImpl,
                    `/api/vertraege/${vertragId}/miet-korrektur`
                )
            ).json()) as MietKorrekturJahr[];
        } finally {
            loading = false;
        }
    }

    $: (vertragId, refresh());

    $: offeneJahre = jahre.filter(
        (j) => j.status === 'Anwendbar' || j.status === 'NkReichtNichtAus'
    );
    $: buchbar = jahre.some((j) => j.status === 'Anwendbar');

    async function differenzBuchen(j: MietKorrekturJahr) {
        busy = true;
        fehler = null;
        const toast = new WalterToastContent(
            'Miet-Korrektur gebucht',
            'Fehler bei der Miet-Korrektur',
            () =>
                `Kaltmiete ${j.jahr} um ${formatEuro(j.differenz)} korrigiert.`,
            (e: unknown) => `Konnte Miet-Korrektur nicht buchen: ${e}`
        );
        try {
            const response = await walter_post(
                `/api/vertraege/${vertragId}/miet-korrektur`,
                { jahr: j.jahr, grund }
            );
            if (response.ok) {
                addToast(toast, true, await response.json());
                grund = '';
                await refresh();
                dispatch('geaendert');
            } else {
                fehler = await response.text();
            }
        } finally {
            busy = false;
        }
    }

    async function loeschen(k: MietKorrekturBuchung) {
        busy = true;
        fehler = null;
        try {
            const resp = await walter_delete(`/api/buchungssaetze/${k.id}`);
            if (resp.ok) {
                await refresh();
                dispatch('geaendert');
            } else fehler = await resp.text();
        } finally {
            busy = false;
        }
    }

    async function stornieren() {
        if (!stornoBuchung) return;
        busy = true;
        fehler = null;
        try {
            const resp = await walter_post(
                `/api/buchungssaetze/${stornoBuchung.id}/storno`,
                { grund: stornoGrund }
            );
            if (resp.ok) {
                await refresh();
                dispatch('geaendert');
            } else fehler = await resp.text();
        } finally {
            stornoBuchung = null;
            stornoGrund = '';
            busy = false;
        }
    }

    function rueckgaengigTitel(k: MietKorrekturBuchung) {
        if (k.kannLoeschen) return 'Korrektur vollständig entfernen';
        if (k.kannStornieren)
            return 'Korrektur stornieren — Korrektur und Storno bleiben sichtbar';
        return k.sperrgrund ?? '';
    }
</script>

{#if loading}
    <InlineLoading description="Prüfe Kaltmiete…" />
{:else if jahre.length > 0}
    {#if offeneJahre.length > 0}
        <InlineNotification
            kind="warning"
            title={`Kaltmiete weicht in ${offeneJahre.length} Jahr(en) vom Vertrag ab`}
            subtitle="Vermutlich wurde eine Mieterhöhung/-senkung rückwirkend nachgetragen. „Differenz buchen“ gleicht die gebuchte Kaltmiete an; der Rest der Zahlungen gilt als NK-Vorauszahlung."
            hideCloseButton
        />
    {/if}
    {#if fehler}
        <InlineNotification
            kind="error"
            title="Aktion nicht möglich"
            subtitle={fehler}
            on:close={() => (fehler = null)}
        />
    {/if}
    <div style="overflow-x: auto; margin-top: 1rem;">
        <table class="bx--data-table" style="min-width: 40rem;">
            <thead>
                <tr>
                    <th>Jahr / Monat</th>
                    <th>Gebucht</th>
                    <th>Laut Vertrag</th>
                    <th>Differenz</th>
                    <th>Status</th>
                    <th />
                </tr>
            </thead>
            <tbody>
                {#each jahre as j (j.jahr)}
                    <tr>
                        <td><strong>{j.jahr}</strong></td>
                        <td />
                        <td />
                        <td><strong>{formatEuro(j.differenz)}</strong></td>
                        <td>
                            <Tag type={statusTag[j.status]}
                                >{statusLabel[j.status]}</Tag
                            >
                        </td>
                        <td>
                            {#if j.status === 'Anwendbar'}
                                <Button
                                    size="small"
                                    disabled={busy || grund.trim().length === 0}
                                    title={grund.trim().length === 0
                                        ? 'Bitte zuerst einen Grund angeben'
                                        : `Kaltmiete ${j.jahr} um ${formatEuro(j.differenz)} korrigieren`}
                                    on:click={() => differenzBuchen(j)}
                                >
                                    Differenz buchen
                                </Button>
                            {/if}
                        </td>
                    </tr>
                    {#each j.monate as m (m.monat)}
                        <tr>
                            <td style="padding-left: 2rem;"
                                >{monatText(j.jahr, m)}</td
                            >
                            <td>{formatEuro(m.istKaltmiete)}</td>
                            <td>{formatEuro(m.sollKaltmiete)}</td>
                            <td>{formatEuro(m.delta)}</td>
                            <td />
                            <td />
                        </tr>
                    {/each}
                    {#each j.korrekturen as k (k.id)}
                        <tr>
                            <td style="padding-left: 2rem;">
                                <a href={`/buchungssaetze/${k.id}`}
                                    >Korrektur vom {formatDate(k.datum)}</a
                                >
                                {#if k.notiz}<br /><small>{k.notiz}</small>{/if}
                            </td>
                            <td>{formatEuro(k.betrag)}</td>
                            <td />
                            <td />
                            <td />
                            <td>
                                <Button
                                    kind="ghost"
                                    size="small"
                                    disabled={busy ||
                                        (!k.kannLoeschen && !k.kannStornieren)}
                                    title={rueckgaengigTitel(k)}
                                    on:click={() =>
                                        k.kannLoeschen
                                            ? loeschen(k)
                                            : (stornoBuchung = k)}
                                >
                                    Rückgängig
                                </Button>
                            </td>
                        </tr>
                    {/each}
                {/each}
            </tbody>
        </table>
    </div>
    {#if buchbar}
        <Row style="margin-top: 1rem; align-items: flex-end;">
            <Column>
                <TextInput
                    required
                    labelText="Grund der Korrektur"
                    placeholder="z.B. rückwirkende Mieterhöhung ab 11/2025"
                    bind:value={grund}
                />
            </Column>
        </Row>
    {/if}
{/if}

<Modal
    open={stornoBuchung !== null}
    modalHeading="Korrektur stornieren"
    primaryButtonText="Stornieren"
    secondaryButtonText="Abbrechen"
    primaryButtonDisabled={busy || stornoGrund.trim().length === 0}
    on:click:button--secondary={() => (stornoBuchung = null)}
    on:close={() => (stornoBuchung = null)}
    on:submit={stornieren}
>
    <p>
        Die Korrektur kann nicht mehr gelöscht werden. Ein Storno bucht sie
        zurück; beide Buchungen bleiben sichtbar.
    </p>
    <TextInput
        required
        labelText="Grund des Stornos"
        bind:value={stornoGrund}
    />
</Modal>
