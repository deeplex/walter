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
    import { WalterGrid, WalterHeader } from '$walter/components';
    import { addToast } from '$walter/store';
    import { Button, FluidForm, PasswordInput } from 'carbon-components-svelte';
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
    <FluidForm style="text-align: center; margin-top: 40vh;">
        <div style="max-width: 40em; margin: auto">
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
            <Button on:click={check_and_update_password}
                >Passwort festlegen</Button
            >
        </div>
    </FluidForm>
</WalterGrid>
