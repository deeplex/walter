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
        InlineNotification,
        Tag
    } from 'carbon-components-svelte';
    import { WalterComboBox, WalterNumberInput } from '$walter/components';
    import { walter_get, walter_selection } from '$walter/services/requests';
    import { convertEuro } from '$walter/services/utils';
    import type {
        AbrechnungsAusgleichInput,
        WalterSelectionEntry
    } from '$walter/lib';

    import { createEventDispatcher, onMount } from 'svelte';

    const dispatch = createEventDispatcher<{
        zahlerResolved: number | undefined;
        zahlungsempfaengerResolved: number | undefined;
    }>();

    export let fetchImpl: typeof fetch;
    export let ausgleich: AbrechnungsAusgleichInput;
    export let invalid = false;

    let vertrag: WalterSelectionEntry | undefined = undefined;
    let jahr: number = new Date().getFullYear() - 1;

    const vertraege = walter_selection.vertraege(fetchImpl);

    type ResultatInfo = {
        id: string;
        jahr: number;
        saldo: number;
        offenerBetrag: number;
        ausgeglichen: boolean;
        abgesendet: boolean;
        vertrag?: { id: number; text: string };
        vermieterBankkontoId?: number;
    };

    // Vermieter-Bankkonto vorbelegen: Zahler bei Erstattung (Geld geht raus),
    // Zahlungsempfänger bei Nachzahlung (Geld kommt an).
    function dispatchBankkonto(r: ResultatInfo) {
        if (r.saldo < 0) {
            dispatch('zahlerResolved', r.vermieterBankkontoId);
        } else if (r.saldo > 0) {
            dispatch('zahlungsempfaengerResolved', r.vermieterBankkontoId);
        }
    }
    let resultat: ResultatInfo | undefined = undefined;
    let fehler: string | undefined = undefined;

    // Vorbelegt (z.B. von der Abrechnungsresultatseite): Resultat direkt laden,
    // Vertrag/Jahr-Auswahl entfällt.
    let vorbelegt = false;
    onMount(async () => {
        if (!ausgleich.abrechnungsresultatId) return;
        vorbelegt = true;
        try {
            const r = (await walter_get(
                `/api/abrechnungsresultate/${ausgleich.abrechnungsresultatId}`,
                fetchImpl
            )) as ResultatInfo;
            resultat = r;
            jahr = r.jahr;
            _prevJahr = r.jahr;
            if (!ausgleich.betrag) {
                ausgleich.betrag = r.offenerBetrag;
            }
            dispatchBankkonto(r);
        } catch {
            fehler = 'Abrechnungsresultat konnte nicht geladen werden.';
        }
    });

    async function ladeResultat() {
        resultat = undefined;
        fehler = undefined;
        ausgleich.abrechnungsresultatId = undefined;
        if (!vertrag?.id || !jahr) return;
        try {
            const r = (await walter_get(
                `/api/abrechnungsresultate/vertrag/${vertrag.id}/jahr/${jahr}`,
                fetchImpl
            )) as ResultatInfo;
            resultat = r;
            if (!r.abgesendet) {
                fehler =
                    'Die Abrechnung wurde noch nicht abgesendet — Ausgleich erst danach möglich.';
                return;
            }
            if (r.ausgeglichen) {
                fehler = 'Die Abrechnung ist bereits ausgeglichen.';
                return;
            }
            ausgleich.abrechnungsresultatId = r.id;
            if (!ausgleich.betrag) {
                ausgleich.betrag = r.offenerBetrag;
            }
            dispatchBankkonto(r);
        } catch {
            fehler = `Keine gebuchte Abrechnung für ${jahr} gefunden.`;
        }
    }

    function onVertragSelect(e: CustomEvent) {
        vertrag = e.detail?.selectedItem;
        void ladeResultat();
    }

    let _prevJahr = jahr;
    $: if (jahr !== _prevJahr) {
        _prevJahr = jahr;
        void ladeResultat();
    }

    $: invalid =
        !ausgleich.abrechnungsresultatId ||
        ausgleich.betrag <= 0 ||
        (resultat != null && ausgleich.betrag > resultat.offenerBetrag + 0.005);
</script>

{#if vorbelegt}
    <Row style="margin-bottom: 0.5rem">
        <Column>
            <strong>
                {resultat?.vertrag?.text ?? 'Abrechnung'} — {resultat?.jahr ??
                    ''}
            </strong>
        </Column>
    </Row>
{:else}
    <Row>
        <Column>
            <WalterComboBox
                required
                titleText="Vertrag"
                entries={vertraege}
                bind:value={vertrag}
                on:select={onVertragSelect}
            />
        </Column>
        <Column>
            <WalterNumberInput
                required
                label="Abrechnungsjahr"
                digits={0}
                bind:value={jahr}
            />
        </Column>
    </Row>
{/if}
{#if fehler}
    <InlineNotification
        kind="warning"
        lowContrast
        hideCloseButton
        title={fehler}
    />
{:else if resultat}
    <Row style="margin-bottom: 0.5rem">
        <Column>
            {#if resultat.saldo > 0}
                <Tag type="red">Nachzahlung des Mieters</Tag>
            {:else}
                <Tag type="teal">Erstattung an den Mieter</Tag>
            {/if}
            <span style="margin-left: 0.5rem">
                Saldo {convertEuro(resultat.saldo)} — offen {convertEuro(
                    resultat.offenerBetrag
                )}
            </span>
        </Column>
    </Row>
    {#if resultat.saldo < 0}
        <InlineNotification
            kind="info"
            lowContrast
            hideCloseButton
            title="Ausgehende Erstattung:"
            subtitle="Optional oben den Zahler (eigenes Bankkonto) angeben — ohne Bankkonto fließt die Auszahlung über das Vertrags-Zahlungskonto ab."
        />
    {/if}
{/if}
<Row>
    <Column>
        <WalterNumberInput
            required
            label="Betrag (€)"
            bind:value={ausgleich.betrag}
        />
    </Column>
    <Column />
</Row>
