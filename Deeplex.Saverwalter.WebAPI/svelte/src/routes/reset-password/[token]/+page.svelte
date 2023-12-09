<script lang="ts">
    import { WalterGrid, WalterHeader } from '$walter/components';
    import { addToast } from '$walter/store';
    import { Button, PasswordInput } from 'carbon-components-svelte';
    import type { PageData } from './$types';
    import { WalterToastContent } from '$walter/lib';
    import { walter_post } from '$walter/services/requests';

    export let data: PageData;

    let new_password_1 = '';
    let new_password_2 = '';
    let new_password_invalid = false;

    async function check_and_update_password() {
        if (new_password_1 !== new_password_2) {
            new_password_invalid = true;
            return;
        }

        const ChangeToast = new WalterToastContent(
            'Passwort erfolgreich geändert',
            'Passwort konnte nicht geändert werden'
        );

        const response = await walter_post(data.apiURL, {
            Token: data.token,
            NewPassword: new_password_1
        });

        addToast(ChangeToast, response.status === 200);
    }
</script>

<WalterHeader title="Passwort setzen" />
<WalterGrid>
    <PasswordInput
        labelText="Neues Passwort eingeben"
        bind:invalid={new_password_invalid}
        invalidText="Passwörter stimmen nicht überein"
        bind:value={new_password_1}
    />
    <PasswordInput
        labelText="Neues Passwort wiederholen"
        bind:invalid={new_password_invalid}
        invalidText="Passwörter stimmen nicht überein"
        bind:value={new_password_2}
    />
    <Button on:click={check_and_update_password}>Passwort festlegen</Button>
</WalterGrid>
