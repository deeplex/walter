<script lang="ts">
    import { ContentSwitcher, Switch } from 'carbon-components-svelte';

    import {
        WalterGrid,
        WalterHeaderNew,
        WalterJuristischePerson,
        WalterNatuerlichePerson
    } from '$WalterComponents';
    import type { PageData } from './$types';
    import type {
        WalterJuristischePersonEntry,
        WalterNatuerlichePersonEntry
    } from '$WalterLib';

    export let data: PageData;

    const apiURL = `/api/kontakte`;
    const title = 'Neue Person';

    const entry: Partial<
        WalterNatuerlichePersonEntry & WalterJuristischePersonEntry
    > = {};

    let personType = 0;
</script>

<WalterHeaderNew
    apiURL={apiURL + `/${personType ? 'jur' : 'nat'}`}
    {title}
    {entry}
>
    <div style="width: 100%;">
        <ContentSwitcher
            style="display: flex; width: 60em; margin: auto"
            size="xl"
            bind:selectedIndex={personType}
        >
            <Switch style="width: 30em" text="Natürliche Person" />
            <Switch style="width: 30em" text="Juristische Person" />
        </ContentSwitcher>
    </div>
</WalterHeaderNew>

<WalterGrid>
    {#if personType === 0}
        <WalterNatuerlichePerson
            a={entry}
            juristischePersonen={data.juristischePersonen}
        />
    {:else}
        <WalterJuristischePerson
            a={entry}
            kontakte={data.kontakte}
            juristischePersonen={data.juristischePersonen}
        />
    {/if}
</WalterGrid>
