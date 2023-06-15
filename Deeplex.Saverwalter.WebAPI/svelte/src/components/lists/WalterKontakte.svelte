<script lang="ts">
    import { goto } from '$app/navigation';
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

    import {
        WalterDataWrapper,
        WalterPerson,
        WalterTextInput
    } from '$walter/components';
    import type {
        WalterJuristischePersonEntry,
        WalterNatuerlichePersonEntry,
        WalterPersonEntry
    } from '$walter/lib';
    import { ContentSwitcher, Row, Switch } from 'carbon-components-svelte';

    const headers = [
        { key: 'name', value: 'Name', default: '' },
        { key: 'adresse.anschrift', value: 'Anschrift' },
        { key: 'telefon', value: 'Telefon' },
        { key: 'mobil', value: 'Mobil' },
        { key: 'email', value: 'E-Mail' }
    ];

    const addUrl = `/api/kontakte/`;

    const navigate = (e: CustomEvent<DataTableRow>) =>
        goto(
            `/kontakte/${e.detail.id > 0 ? 'nat' : 'jur'}/${Math.abs(
                e.detail.id
            )}`
        );

    export let rows: WalterPersonEntry[];
    export let search = false;
    export let title: string | undefined = undefined;

    let personType = 0;

    export let entry:
        | Partial<WalterNatuerlichePersonEntry & WalterJuristischePersonEntry>
        | undefined = undefined;
</script>

<WalterDataWrapper
    {addUrl}
    addEntry={entry}
    {title}
    {search}
    {navigate}
    {rows}
    {headers}
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
