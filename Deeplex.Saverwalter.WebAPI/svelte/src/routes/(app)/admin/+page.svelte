<script lang="ts">
    import { WalterHeader } from '$walter/components';
    import { page } from '$app/stores';
    import { walter_post } from '$walter/services/requests';
    import {
        Content, Tile,
    } from 'carbon-components-svelte';
    import WalterDataTable from '$walter/components/elements/WalterDataTable.svelte';
    import { walter_goto } from '$walter/services/utils';
    import { navigation } from '$walter/services/navigation';
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

    function create_user_account()
    {
        walter_post('/api/admin/create', {user_name: user_name});
    }

    let user_name = '';

    const headers = [
        { key: 'username', value: 'Nutzername' },
        { key: 'name', value: 'Anzeigename' },
        { key: 'passwordlink', value: 'Passwortlink' },
    ];

    const rows = [
        {
            "id": 1,
            "username": "admin",
            "name": "Administrator",
            "passwordlink": "",
        },
        {
            "id": 2,
            "username": "user",
            "name": "Nutzer",
            "passwordlink": "whatever",
        }
    ];

    const whatever = $page.url.host;

    function add() {
        walter_goto(`account/new`);
    }

    const on_click_row = (e: CustomEvent<DataTableRow>) =>
        navigation.account(e.detail.id);
</script>

<WalterHeader title="Nutzereinstellungen" />

<Content>
    <Tile>
        <h3>Nutzertabelle</h3>
    </Tile>
    <WalterDataTable
        {add}
        {on_click_row}
        {rows}
        {headers}/>
</Content>
