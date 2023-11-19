<script lang="ts">
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

    import {
        WalterDataWrapper,
        WalterPerson,
        WalterTextInput
    } from '$walter/components';
    import {
        WalterPersonEntry,
        type WalterJuristischePersonEntry,
        type WalterNatuerlichePersonEntry
    } from '$walter/lib';
    import { ContentSwitcher, Row, Switch } from 'carbon-components-svelte';
    import { navigation } from '$walter/services/navigation';

    const headers = [
        { key: 'name', value: 'Name', default: '' },
        { key: 'adresse.anschrift', value: 'Anschrift' },
        { key: 'telefon', value: 'Telefon' },
        { key: 'mobil', value: 'Mobil' },
        { key: 'email', value: 'E-Mail' }
    ];

    const on_click_row = (e: CustomEvent<DataTableRow>) =>
        e.detail.id > 0
            ? navigation.natuerlicheperson(e.detail.id)
            : navigation.juristischeperson(Math.abs(e.detail.id));

    export let rows: WalterPersonEntry[];
    export let fullHeight = false;
    export let title: string | undefined = undefined;

    let personType = 0;

    export let entry:
        | Partial<WalterNatuerlichePersonEntry & WalterJuristischePersonEntry>
        | undefined = undefined;
</script>

<WalterDataWrapper
    addUrl={WalterPersonEntry.ApiURL}
    addEntry={entry}
    {title}
    {on_click_row}
    {rows}
    {headers}
    {fullHeight}
>
    {#if entry}
        <Row>
            <ContentSwitcher bind:selectedIndex={personType}>
                <Switch text="Natürliche Person" />
                <Switch text="Juristische Person" />
            </ContentSwitcher>
        </Row>
        <Row>
            {#if personType === 0}
                <WalterTextInput
                    bind:value={entry.vorname}
                    labelText="Vorname"
                />
                <WalterTextInput
                    bind:value={entry.nachname}
                    labelText="Nachname"
                />
            {:else}
                <WalterTextInput
                    bind:value={entry.name}
                    labelText="Bezeichnung"
                />
            {/if}
        </Row>
        <WalterPerson value={entry} />
    {/if}
</WalterDataWrapper>
