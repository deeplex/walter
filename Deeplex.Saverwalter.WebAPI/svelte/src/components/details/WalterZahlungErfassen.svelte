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
    Schlankes Erfassen einer Zahlung für ein Ziel aus dem Kontext — Miete eines Monats,
    Garagenmiete, Zahlung einer erfassten oder neuen Betriebskostenrechnung oder einer
    Rechnung für Erhaltungsaufwand, Nachzahlung oder Erstattung einer
    Nebenkostenabrechnung: Datum, Betrag, ein Bankkonto und ein Verwendungszweck. Das
    Bankkonto ist bei einer Erstattung der Empfänger, sonst der Zahler; bei einer Rechnung
    für Erhaltungsaufwand ist es Pflicht (vorbelegt mit dem Bankkonto des Vermieters).
    Wie eine
    Mietzahlung auf Kaltmiete, Garagen und NK-Vorauszahlung verteilt wird, entscheidet
    der Server; die Vorschau zeigt es vor dem Speichern. „Erweitert …“ öffnet das volle
    Transaktionsformular mit denselben Werten. Event `gebucht` nach dem Speichern.
-->
<script lang="ts">
    import {
        Button,
        InlineNotification,
        Modal,
        TextInput
    } from 'carbon-components-svelte';
    import {
        WalterComboBox,
        WalterDatePicker,
        WalterNumberInput
    } from '$walter/components';
    import WalterTransaktion from './WalterTransaktion.svelte';
    import WalterBuchungsvorschau from './WalterBuchungsvorschau.svelte';
    import WalterMatrixDetailInhalt from '../elements/WalterMatrixDetailInhalt.svelte';
    import type { MatrixDetail } from '$walter/services/matrix';
    import {
        emptyTransaktionsInput,
        type TransaktionsInput,
        type WalterSelectionEntry,
        type WalterZahlungsZiel
    } from '$walter/lib';
    import { convertDateCanadian } from '$walter/services/utils';
    import { walter_post, walter_selection } from '$walter/services/requests';
    import { changeTracker } from '$walter/store';
    import { createEventDispatcher } from 'svelte';
    import { get } from 'svelte/store';

    export let fetchImpl: typeof fetch;
    export let ziel: WalterZahlungsZiel;
    /** Wofür, für die Überschrift, z. B. „Mär 2026“ oder der Name der Rechnung. */
    export let titel: string;
    /** Vorbelegter Betrag (offener Rest). */
    export let vorschlag = 0;
    /**
     * Bankkonto des Vermieters, falls bekannt: Empfänger der Zahlung, bei einer
     * Erstattung oder einer Rechnung für Erhaltungsaufwand der Zahler.
     */
    export let vermieterBankkontoId: number | undefined = undefined;
    export let open = false;
    /**
     * Was für das Feld schon eingetragen ist. Steht es da, zeigt der Dialog es über
     * dem Formular, damit man Vorhandenes und Neues in einem Dialog sieht.
     */
    export let detail: MatrixDetail | undefined = undefined;
    export let detailLaden = false;
    export let detailLeerText = 'Noch nichts eingetragen.';
    /** Das Feld ist erledigt: das Formular bleibt zu, bis man mehr erfassen will. */
    export let formularZu = false;
    export let weitereAktion = 'Weitere Zahlung erfassen';

    const dispatch = createEventDispatcher<{ gebucht: void }>();
    const bankkonten = walter_selection.bankkontos(fetchImpl);

    let datum: string | undefined;
    let betrag: number | undefined;
    /** Bankkonto der Gegenseite (Mieter bzw. Zahler einer Rechnung). */
    let gegenkonto: WalterSelectionEntry | undefined;
    let verwendungszweck = '';
    let erweitert = false;
    let erweitertInput: TransaktionsInput = emptyTransaktionsInput();
    let erweitertGueltig = false;
    let vorschauGueltig = false;
    let busy = false;
    /** Bei einem erledigten Feld bleibt das Formular zunächst zu. */
    let formularSichtbar = true;
    let fehler: string | null = null;

    // Die Eingabefelder melden Änderungen an den Seiten-Änderungszähler; der Dialog
    // stellt ihn beim Schließen wieder her, damit die Seite nicht als geändert gilt.
    let tracker = 0;
    let warOffen = false;
    $: if (open && !warOffen) {
        warOffen = true;
        tracker = get(changeTracker);
        datum = convertDateCanadian(new Date());
        betrag = vorschlag > 0 ? Math.round(vorschlag * 100) / 100 : undefined;
        gegenkonto = undefined;
        if (ziel.art === 'erhaltungsaufwand' && vermieterBankkontoId)
            vorbelegen(vermieterBankkontoId);
        verwendungszweck = '';
        erweitert = false;
        fehler = null;
        formularSichtbar = !(detail && formularZu);
    } else if (!open && warOffen) {
        warOffen = false;
        changeTracker.set(tracker);
    }

    async function vorbelegen(id: number) {
        const konto = (await bankkonten).find((b) => b.id === id);
        if (konto && !gegenkonto) gegenkonto = konto;
    }

    /** Positionen zum Ziel; `aufteilen`: der Server teilt eine Mietzahlung auf. */
    function positionen(
        betrag: number,
        datum: string,
        aufteilen: boolean
    ): Partial<TransaktionsInput> {
        switch (ziel.art) {
            case 'miete':
                return {
                    mieten: [
                        {
                            vertragId: ziel.vertragId,
                            betreffenderMonat: ziel.monat,
                            kaltmiete: 0,
                            garagen: [],
                            nkVorauszahlung: 0,
                            ...(aufteilen ? { gesamtbetrag: betrag } : {})
                        }
                    ]
                };
            case 'garage':
                return {
                    garagenEingaenge: [
                        {
                            garageVertragId: ziel.garageVertragId,
                            garageKennung: ziel.kennung ?? '',
                            betreffenderMonat: ziel.monat,
                            betrag
                        }
                    ]
                };
            case 'bkRechnung':
                return {
                    betriebskostenEingaenge: [
                        { existingBuchungssatzId: ziel.buchungssatzId, betrag }
                    ]
                };
            case 'bkNeu':
                return {
                    betriebskostenEingaenge: [
                        {
                            umlageId: ziel.umlageId,
                            betreffendesJahr: ziel.jahr,
                            rechnungsDatum: datum,
                            betrag
                        }
                    ]
                };
            case 'abrechnung':
                return {
                    abrechnungsAusgleiche: [
                        {
                            abrechnungsresultatId: ziel.abrechnungsresultatId,
                            betrag
                        }
                    ]
                };
            case 'erhaltungsaufwand':
                return {
                    erhaltungsaufwendungen: [
                        {
                            existingBuchungssatzId: ziel.buchungssatzId,
                            wohnungId: ziel.wohnungId,
                            betrag
                        }
                    ]
                };
        }
    }

    $: auszahlung = ziel.art === 'abrechnung' && ziel.erstattung;
    /** Der Vermieter zahlt eine Rechnung: das gewählte Bankkonto ist der Zahler. */
    $: rechnungZahlen = ziel.art === 'erhaltungsaufwand';

    function eingabeFuer(aufteilen: boolean): TransaktionsInput {
        return {
            ...emptyTransaktionsInput(),
            betrag: betrag ?? 0,
            zahlungsdatum: datum ?? '',
            zahlerId: auszahlung
                ? vermieterBankkontoId
                : (gegenkonto?.id as number | undefined),
            zahlungsempfaengerId: auszahlung
                ? (gegenkonto?.id as number | undefined)
                : rechnungZahlen
                  ? undefined
                  : vermieterBankkontoId,
            verwendungszweck,
            ...positionen(betrag ?? 0, datum ?? '', aufteilen)
        };
    }

    // Alle Felder, die eingabeFuer() liest, als Abhängigkeiten nennen.
    $: einfacheEingabe = neuBerechnen(
        datum,
        betrag,
        gegenkonto,
        verwendungszweck,
        vermieterBankkontoId,
        ziel,
        auszahlung,
        rechnungZahlen
    );
    function neuBerechnen(...felder: unknown[]): TransaktionsInput | undefined {
        void felder;
        return datum && betrag && betrag > 0 && (!rechnungZahlen || gegenkonto)
            ? eingabeFuer(true)
            : undefined;
    }

    function erweitern() {
        erweitertInput = eingabeFuer(false);
        erweitert = true;
    }

    $: eingabe = erweitert
        ? erweitertGueltig
            ? erweitertInput
            : undefined
        : einfacheEingabe;
    $: buchbar = !!eingabe && vorschauGueltig && !busy;

    async function buchen() {
        if (!eingabe || !buchbar) return;
        busy = true;
        fehler = null;
        try {
            const response = await walter_post(
                '/api/transaktionen/buchen',
                eingabe
            );
            if (response.ok) {
                open = false;
                dispatch('gebucht');
            } else {
                fehler = await response.text();
            }
        } finally {
            busy = false;
        }
    }
</script>

<Modal
    bind:open
    size={erweitert || detail ? 'lg' : 'sm'}
    modalHeading={detail ? titel : `Zahlung erfassen – ${titel}`}
    primaryButtonText="Buchen"
    secondaryButtonText="Abbrechen"
    primaryButtonDisabled={!buchbar}
    hasScrollingContent={erweitert}
    on:click:button--secondary={() => (open = false)}
    on:submit={buchen}
>
    {#if open}
        {#if detail}
            <WalterMatrixDetailInhalt
                {detail}
                loading={detailLaden}
                leerText={detailLeerText}
            />
            {#if !formularSichtbar}
                <Button
                    kind="tertiary"
                    size="small"
                    style="margin-top: 1rem"
                    on:click={() => (formularSichtbar = true)}
                >
                    {weitereAktion}
                </Button>
            {:else}
                <h5 class="neu">Neu erfassen</h5>
            {/if}
        {/if}

        {#if !formularSichtbar}
            <!-- Formular zu: nichts offen -->
        {:else if erweitert}
            <WalterTransaktion
                {fetchImpl}
                bind:buchung={erweitertInput}
                bind:isValid={erweitertGueltig}
            />
        {:else}
            <WalterDatePicker required bind:value={datum} labelText="Datum" />
            <WalterNumberInput
                required
                hideSteppers
                label="Betrag (€)"
                bind:value={betrag}
            />
            <WalterComboBox
                required={rechnungZahlen}
                titleText={auszahlung
                    ? 'Bankkonto des Empfängers (optional)'
                    : rechnungZahlen
                      ? 'Bankkonto des Zahlers'
                      : 'Bankkonto des Zahlers (optional)'}
                entries={bankkonten}
                bind:value={gegenkonto}
            />
            <TextInput
                labelText="Verwendungszweck (optional)"
                bind:value={verwendungszweck}
            />
            <Button
                kind="ghost"
                size="small"
                style="margin-top: 0.5rem"
                on:click={erweitern}
            >
                Erweitert …
            </Button>
        {/if}

        {#if formularSichtbar}
            <WalterBuchungsvorschau
                input={eingabe}
                bind:gueltig={vorschauGueltig}
            />

            {#if fehler}
                <InlineNotification
                    kind="error"
                    title="Buchung nicht möglich:"
                    subtitle={fehler}
                    hideCloseButton
                    lowContrast
                />
            {/if}
        {/if}
    {/if}
</Modal>

<style>
    .neu {
        margin: 1.5rem 0 0.5rem;
    }
</style>
