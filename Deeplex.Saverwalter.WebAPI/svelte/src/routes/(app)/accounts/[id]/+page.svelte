<script lang="ts">
    import { WalterGrid, WalterHeaderDetail } from '$walter/components';
    import WalterAccount from '$walter/components/details/WalterAccount.svelte';
    import { WalterS3FileWrapper } from '$walter/lib';
    import { Button, CodeSnippet, Row } from 'carbon-components-svelte';
    import type { PageData } from './$types';
    import { walter_post } from '$walter/services/requests';

    export let data: PageData;

    const title = `${data.entry.username} - ${data.entry.name}`;

    let fileWrapper = new WalterS3FileWrapper(data.fetchImpl);
    fileWrapper.registerStack();
    fileWrapper.register(title, data.S3URL);

    let passwordResetLink = '';

    async function get_password_link() {
        const token = await walter_post(
            `/api/accounts/${data.entry.id}/reset-credentials`,
            {}
        ).then((r) => r.text());
        passwordResetLink = `${window.location.origin}/reset-password/${token}`;
    }
    const permissions = {
        read: true,
        update: true,
        remove: true
    };
</script>

<WalterHeaderDetail
    entry={{ ...data.entry, permissions }}
    apiURL={data.apiURL}
    {title}
    bind:fileWrapper
/>

<WalterGrid>
    <WalterAccount admin entry={data.entry} fetchImpl={data.fetchImpl} />
    <div style="margin: 1em">
        <Row>
            <Button on:click={get_password_link} kind="danger"
                >Passwortlink anfordern</Button
            >
            <CodeSnippet code={passwordResetLink} />
        </Row>
    </div>
</WalterGrid>
