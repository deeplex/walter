<script lang="ts">
    import { WalterDataTable } from '$walter/components';
    import type { WalterBetriebskostenrechnungEntry } from '$walter/lib';
    import { convertDateCanadian } from '$walter/services/utils';
    import type { WalterRechnungEntry } from '$walter/types/WalterBetriebskostenabrechnung.type';
    import WalterBetriebskostenrechnung from '../details/WalterBetriebskostenrechnung.svelte';
    import WalterDataWrapperQuickAdd from '../elements/WalterDataWrapperQuickAdd.svelte';
    import { goto_or_create_rechnung } from './WalterAbrechnungGruppe';

    export let rows: WalterRechnungEntry[];
    export let year: number;
    export let fetchImpl: typeof fetch;

    let quickAddEntry: Partial<WalterBetriebskostenrechnungEntry> = {};

    const headers = [
        { key: 'typ', value: 'Kostenanteil' },
        { key: 'schluessel', value: 'Schlüssel' },
        { key: 'gesamtBetrag', value: 'Betrag' },
        { key: 'betragLetztesJahr', value: 'Betrag (letztes Jahr)' },
        { key: 'anteil', value: 'Anteil' },
        { key: 'betrag', value: 'Kosten' },
        { key: 'button', value: 'Hinzufügen' }
    ];

    function add(e: CustomEvent, rechnung: WalterRechnungEntry) {
        e.stopPropagation();

        quickAddEntry = {
            betrag: rechnung.gesamtBetrag || rechnung.betragLetztesJahr,
            betreffendesJahr: year,
            datum: convertDateCanadian(new Date()),
            typ: { id: `${rechnung.typId}`, text: rechnung.typ },
            umlage: { id: `${rechnung.id}`, text: rechnung.typ },
            id: rechnung.rechnungId || undefined
        };

        open = true;
    }

    const rowsAdd = rows.map((row) => ({
        ...row,
        button: row.rechnungId ? 'disabled' : (e: CustomEvent) => add(e, row)
    }));

    const navigate = (e: any) => {
        goto_or_create_rechnung(e.detail, year);
    };

    let open = false;
</script>

<WalterDataWrapperQuickAdd
    title={quickAddEntry.typ?.text || 'Rechnung'}
    addEntry={quickAddEntry}
    addUrl={quickAddEntry.id ? '' : '/api/betriebskostenrechnung/'}
    bind:addModalOpen={open}
>
    <WalterBetriebskostenrechnung
        readonly={quickAddEntry.id !== undefined}
        {fetchImpl}
        entry={quickAddEntry}
    />
</WalterDataWrapperQuickAdd>

<WalterDataTable size="short" fullHeight {navigate} {headers} rows={rowsAdd} />
