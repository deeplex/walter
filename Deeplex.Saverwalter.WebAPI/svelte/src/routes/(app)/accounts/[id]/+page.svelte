<!-- Copyright (C) 2023-2024  Kai Lawrence -->
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
    import { WalterGrid, WalterHeaderDetail } from '$walter/components';
    import WalterAccount from '$walter/components/details/WalterAccount.svelte';
    import { WalterFileWrapper } from '$walter/lib';
    import { Button, CodeSnippet, Row } from 'carbon-components-svelte';
    import type { PageData } from './$types';
    import { walter_post } from '$walter/services/requests';

    export let data: PageData;

    let title = `${data.entry.username} - ${data.entry.name}`;
    $: {
        title = `${data.entry.username} - ${data.entry.name}`;
    }

    let fileWrapper = new WalterFileWrapper(data.fetchImpl);
    fileWrapper.registerStack();
    fileWrapper.register(title, data.fileURL);

    let passwordResetLink = '';

    async function get_password_link() {
        const token = await walter_post(
            `/api/accounts/${data.entry.id}/reset-credentials`,
            {}
        ).then((r) => r.text());
        passwordResetLink = `${window.location.origin}/reset-password/${token}`;
    }
</script>

<WalterHeaderDetail
    bind:entry={data.entry}
    apiURL={data.apiURL}
    {title}
    bind:fileWrapper
/>

<WalterGrid>
    <WalterAccount admin entry={data.entry} fetchImpl={data.fetchImpl} />
    <div style="margin: 1em">
        <Row>
            <Button size="small" on:click={get_password_link} kind="danger"
                >Passwortlink anfordern</Button
            >
            <CodeSnippet code={passwordResetLink} />
        </Row>
    </div>
</WalterGrid>
